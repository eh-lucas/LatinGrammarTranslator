# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Latin Grammar Translator is a microservices system (C#/.NET + Python) for translating Allen & Greenough's Latin Grammar from English to Portuguese. The system uses AI translation with specialized parsing to preserve academic terminology and document structure, and generates professional Word documents with customizable themes.

**Architecture:** Docker-based microservices with Python translator service and .NET API for orchestration and Word generation.

## Development Commands

### Quick Start (Docker - Recommended)

```bash
# 1. Configure API key
cp .env.example .env
# Edit .env and add your TRANSLATOR_API_KEY

# 2. Start all services
docker-compose up --build

# 3. Access applications
# - Swagger UI (.NET): http://localhost:8080/swagger
# - Python API: http://localhost:5001
# - Health check: http://localhost:5001/health

# Stop services
docker-compose down
```

### Python Service (Local Development)

```bash
cd src/PythonTranslator

# Setup environment
python -m venv .venv
source .venv/bin/activate  # Linux/Mac
.venv\Scripts\activate     # Windows
pip install -r requirements.txt

# Configure via environment variables
export TRANSLATOR_API_KEY=your_key_here
export TRANSLATOR_PROVIDER=gemini  # or "claude"
export TRANSLATOR_MODEL=gemini-3-pro-preview  # optional

# Run Flask API
python app.py  # Runs on http://0.0.0.0:5001

# Testing
python test_parser.py              # Parser only (no AI)
python test_translation.py         # With AI translation
python test_full_pipeline.py       # Full pipeline (parse + translate + generate)
```

### .NET API (Local Development)

```bash
cd src/Api/LatinGrammarTranslator

# Build and run
dotnet build
dotnet run  # Runs on http://localhost:5287 (or port from launchSettings.json)

# Access Swagger UI
# http://localhost:5287/swagger
```

## System Architecture

### Service Overview

```
┌─────────────────────────────────────────────┐
│           Client (Browser/API)              │
└──────────────────┬──────────────────────────┘
                   │
         ┌─────────┴─────────┐
         │                   │
┌────────▼────────┐  ┌───────▼────────────┐
│  .NET API       │  │  Python Translator │
│  Port 8080      │  │  Port 5001         │
│                 │  │                    │
│ • Swagger UI    │  │ • HTML Parser      │
│ • Word Gen      │  │ • AI Translation   │
│ • 4 Themes      │  │ • Gemini/Claude    │
│ • DocumentFormat│  │ • Strategy Pattern │
│   .OpenXml      │  │ • Flask API        │
└─────────────────┘  └────────────────────┘
```

### Translation Pipeline Flow

```
HTML Document → LatinGrammarParser → ParsedDocument (with typed segments)
                                           ↓
                              SectionTranslator + Strategy Pattern
                                           ↓
                    AI Provider (Gemini/Claude) with glossary context
                                           ↓
                              Translated ParsedDocument (JSON)
                                           ↓
                              ├─→ HtmlGenerator → .html output
                              └─→ .NET API → WordDocumentBuilder → .docx output
                                                    ↓
                                  Professional Word document with themes:
                                  • Academic (Times New Roman, mirrored margins)
                                  • Modern (Calibri, blue headings)
                                  • Compact (Arial, minimal margins)
                                  • Classic (Garamond, 19th century style)
```

### API Endpoints

**Python Service (port 5001):**
- `POST /parse-html` - Parse HTML and return structure
- `POST /translate` - Parse and translate HTML or ParsedDocument
- `GET /health` - Health check
- `GET /stats` - Service information

**. NET API (port 8080):**
- `POST /generate-word` - Generate Word document from ParsedDocument
- `GET /themes` - List available Word themes
- `GET /test-word-generation` - Generate test documents with all themes
- `POST /process` - Legacy endpoint (orchestrates parsing + translation)
- Swagger UI: `/swagger`

## Critical Design Patterns

### Strategy Pattern for AI Providers

- `TranslationStrategy` (abstract base in `translation_strategy.py`)
- `GeminiTranslator` (gemini_translator.py) - free tier, good quality
- `ClaudeTranslator` (claude_translator.py) - paid, best quality
- `TranslatorFactory` creates appropriate strategy based on provider name

**Configuration via environment variables:**
- `TRANSLATOR_PROVIDER`: "gemini" or "claude"
- `TRANSLATOR_API_KEY`: API key for selected provider
- `TRANSLATOR_MODEL`: Optional specific model (uses provider default if not set)

### Builder Pattern for Word Generation (.NET)

**Fluent API with method chaining:**
```csharp
var doc = WordDocumentBuilder.Create(outputPath)
    .LoadTheme(theme)
    .AddTitlePage("Document Title")
    .AddHeading("Chapter 1", level: 1)
    .AddParagraph("Content here")
    .AddTable(t => t.AddHeaderRow("Col1", "Col2").AddRow("A", "B"))
    .Build();
```

**Components:**
- `WordDocumentBuilder` - Main builder with Fluent API
- `ParagraphBuilder` - Build formatted paragraphs
- `TableBuilder` - Build tables with borders and styling
- `RunBuilder` - Build text runs with formatting
- `StylesManager` - Manage document styles via StyleDefinitionsPart
- `PageLayoutManager` - Configure page size, orientation, mirrored margins
- `ConfigurationLoader` - Load and cache theme JSON files
- `ConfigurationValidator` - Strict validation with descriptive exceptions

### Section-Based Translation

The system translates entire HTML sections (full documents) in one API call to maintain context. This is **critical** for consistency of academic terminology across paragraphs. Do not refactor to per-paragraph translation without understanding this design decision.

## Python Service Components

**html_parser.py (`LatinGrammarParser`):**
- Parses HTML using BeautifulSoup with lxml
- Classifies text segments: `LATIN` (preserve), `ENGLISH` (translate), `GLOSS` (translate)
- Preserves formatting (bold, italic, tables, lists)
- Detection logic: CSS classes (`foreign`, `gloss`) and language detection for fallback
- **Ignores HTML comments** (commit 1cc7f42)

**models.py:**
- Pydantic models: `ParsedDocument`, `ParsedNode`, `TextSegment`, `TableStructure`
- `TextType` enum: LATIN, ENGLISH, GLOSS, REFERENCE, MIXED
- `NodeType` enum: PARAGRAPH, HEADING, LIST_ITEM, TABLE, etc.
- `FormattingStyle`: bold, italic, underline, colors, fonts

**glossary.py:**
- 100+ technical grammar terms (Nominative→Nominativo, Conjugation→Conjugação)
- Injected into AI prompts for consistent terminology
- Edit this file to add new term mappings

**translation_strategy.py (`SectionTranslator`):**
- Orchestrates full document translation
- Extracts all translatable segments with unique IDs
- Calls `strategy.translate_section()` with full context
- Applies translations back to ParsedDocument using segment ID mapping
- Includes retry logic (default: 3 attempts)

**gemini_translator.py / claude_translator.py:**
- Implement `translate_section()` method from `TranslationStrategy`
- Build prompts with document context + glossary + structured segment list
- Parse JSON responses back into `TranslationResult`
- Handle API-specific error codes and rate limits

**html_generator.py (`HtmlGenerator`):**
- Reconstructs translated HTML from ParsedDocument
- Preserves document structure, CSS classes, inline styles
- Generates complete HTML with proper DOCTYPE and head section

**app.py (Flask API):**
- `/parse-html` - Parse HTML only
- `/translate` - Parse and translate (accepts HTML or ParsedDocument JSON)
- `/health` - Health check for Docker
- `/stats` - Service statistics and configuration info

## .NET API Components

**Program.cs:**
- Minimal API with Swagger enabled (`UseSwagger()`, `UseSwaggerUI()`)
- Services registered: `DocumentGenerationService`, `TranslationService`, `HtmlService`
- Endpoints: `/generate-word`, `/themes`, `/test-word-generation`, `/process`
- HttpClient configured to call Python service at `http://translator:5001`

**Services/WordGeneration/:**

**WordDocumentBuilder.cs:**
- Main Fluent API builder
- Static factory: `WordDocumentBuilder.Create(filePath)`
- Methods: `LoadTheme()`, `AddTitlePage()`, `AddHeading()`, `AddParagraph()`, `AddTable()`, `Build()`
- Implements IDisposable for proper resource cleanup

**Configuration/**
- `DocumentConfiguration` - Page size, orientation, margins (mirrored for book layout)
- `StyleConfiguration` - Individual style properties (font, size, colors, spacing)
- `ThemeConfiguration` - Aggregates layout + styles
- `ConfigurationLoader` - Loads themes from JSON with caching
- `ConfigurationValidator` - Strict validation (margins 0.5-10cm, fonts from whitelist, hex colors)

**Builders/**
- `ParagraphBuilder` - Create paragraphs with styles and runs
- `TableBuilder` - Create tables with headers, data rows, borders
- `RunBuilder` - Create formatted text runs (bold, italic, colors)

**Styles/**
- `StylesManager` - Manages document styles via StyleDefinitionsPart
- `StyleDefinition` - Fluent builder for OpenXml Style objects
- Supports: Normal, headings (1-4), Latin, Gloss, Section, Footnote, Quote, ListParagraph, TableNormal

**Layout/**
- `PageLayoutManager` - Page size and margins
- `SetMirroredMargins()` - Book layout (inner = binding side, outer = page edge)
- `SetStandardMargins()` - Normal left/right margins
- Supports A4, A5, Letter, Legal page sizes

**Formatting/**
- `FormattingHelper` - Unit conversion utilities
- `CentimetersToTwips()`, `InchesToTwips()`, `PointsToHalfPoints()`, `LineSpacingToOpenXml()`

**DocumentGenerationService.cs:**
- Main service for generating Word from ParsedDocument
- `GenerateDocument(parsedDoc, outputPath, themeName)`
- Processes node hierarchy recursively
- Handles: headings, paragraphs, tables, lists, blockquotes
- Extracts text from TextSegments with proper formatting

**Themes/ (JSON):**
- `academic.json` - Times New Roman, 12pt, mirrored margins (3cm inner, 2cm outer)
- `modern.json` - Calibri, 11pt, blue headings, standard margins
- `compact.json` - Arial, 10pt, minimal margins (1.5cm)
- `classic.json` - Garamond, 12pt, generous margins, 19th century style

**Models/ParsedDocument.cs:**
- C# mirror of Python Pydantic models
- JSON serialization with `JsonPropertyName` attributes
- Enums: `NodeType`, `TextType`
- Classes: `ParsedDocument`, `ParsedNode`, `TextSegment`, `FormattingStyle`

## Docker Configuration

**docker-compose.yml:**
- Orchestrates both services with shared network
- Volumes for output files and theme configs
- Health checks on Python service
- Environment variables from `.env` file

**Dockerfile (Python):**
- Python 3.11-slim base image
- Installs requirements.txt dependencies
- Exposes port 5001
- Production Flask configuration

**Dockerfile (.NET):**
- Multi-stage build (SDK for build, runtime for final)
- Copies Themes/ directory to output
- Exposes ports 8080, 8081

**.env Configuration:**
- `TRANSLATOR_PROVIDER` - "gemini" or "claude"
- `TRANSLATOR_API_KEY` - API key (required)
- `TRANSLATOR_MODEL` - Optional specific model
- Never commit .env (gitignored), use .env.example as template

## Important Constraints

1. **Preserve Latin text exactly** - TextType.LATIN segments must never be translated
2. **Use glossary terms consistently** - Always check glossary.py before translating grammar terms
3. **Maintain document structure** - Node hierarchy and formatting must survive translation
4. **Section-level context** - Do not split translations into smaller units
5. **API keys required** - System needs TRANSLATOR_API_KEY environment variable
6. **Mirrored margins for books** - Use inner/outer, not left/right for book layouts
7. **Theme validation** - All theme JSONs must pass ConfigurationValidator checks
8. **Twips for measurements** - OpenXml uses twips (1/1440 inch), not points or pixels

## Testing Strategy

### Python Tests
```bash
cd src/PythonTranslator

# Parser only (no AI calls, free)
python test_parser.py

# Translation with AI
python test_translation.py --provider gemini

# Full pipeline
python test_full_pipeline.py
python test_full_pipeline.py --skip-translation  # Skip AI (free)
```

### .NET Tests
```bash
cd src/Api/LatinGrammarTranslator

# Build and run
dotnet build
dotnet run

# Access test endpoint via Swagger or direct:
curl http://localhost:5287/test-word-generation
```

### Integration Tests (Docker)
```bash
# Start all services
docker-compose up

# Test Python health
curl http://localhost:5001/health

# Test .NET Swagger
# Open http://localhost:8080/swagger in browser
```

## Resource Files

Source HTML files are in `src/Resources/` (e.g., alphabet.htm, syntax.htm). These are the actual Allen & Greenough grammar chapters to be translated.

## Recent Major Changes

**2025-11-22:**
- Implemented robust Word generation in .NET using DocumentFormat.OpenXml
- Added Builder Pattern with Fluent API for document construction
- Created 4 professional themes with JSON configuration
- Integrated Docker Compose for full microservices orchestration
- Added Swagger UI to .NET API
- Added `/translate` endpoint to Python service
- Migrated from python-docx to .NET Word generation (production-ready)

## Known Technical Debt

- No caching layer for translations (re-running wastes API credits)
- No batch processing optimization for multiple files
- C# HtmlService is simplified stub (main parsing in Python)
- No database for storing translations
- No authentication/authorization on APIs
