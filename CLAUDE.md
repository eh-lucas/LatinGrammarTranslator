# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Latin Grammar Translator is a bilingual system (C#/.NET + Python) for translating Allen & Greenough's Latin Grammar from English to Portuguese. The system uses AI translation with specialized parsing to preserve academic terminology and document structure.

**Architecture:** Microservices-style with a C# API layer and Python translation service communicating via HTTP.

## Development Commands

### Python Service (Primary Translation Engine)

```bash
# Setup Python environment
cd src/PythonTranslator
pip install -r requirements.txt

# Configure API keys
set GEMINI_API_KEY=your_key_here        # Windows (recommended for testing - free)
export GEMINI_API_KEY=your_key_here     # Linux/Mac
set CLAUDE_API_KEY=your_key_here        # Optional (better quality, costs money)

# Run Flask API
python app.py                           # Runs on http://0.0.0.0:5001

# Test parser only (no AI calls)
python test_parser.py

# Test translation with AI
python test_translation.py              # Uses alphabet.htm by default
python test_translation.py ../Resources/syntax.htm --output result.json
python test_translation.py --provider claude  # Use Claude instead of Gemini

# Test full pipeline (parse + translate + HTML + Word generation)
python test_full_pipeline.py
python test_full_pipeline.py --skip-translation  # Test without AI (free)
python test_full_pipeline.py --output result.docx --output-html custom.html
```

### C# API (Orchestrator Layer)

```bash
cd src/Api/LatinGrammarTranslator

# Build
dotnet build

# Run
dotnet run

# Note: C# API expects Python service at http://translator:5001 (Docker) or needs configuration update
```

## System Architecture

### Translation Pipeline Flow

```
HTML Document → LatinGrammarParser → ParsedDocument (with typed segments)
                                           ↓
                              SectionTranslator + Strategy Pattern
                                           ↓
                              AI Provider (Gemini/Claude) with glossary context
                                           ↓
                              Translated ParsedDocument
                                           ↓
                              ├─→ HtmlGenerator → .html output (translated)
                              └─→ SimpleWordGenerator → .docx output
```

### Critical Design Patterns

**Strategy Pattern for AI Providers:**
- `TranslationStrategy` (abstract base in `translation_strategy.py`)
- `GeminiTranslator` (gemini_translator.py) - free tier, good quality
- `ClaudeTranslator` (claude_translator.py) - paid, best quality
- `TranslatorFactory` creates appropriate strategy based on provider name

**Section-Based Translation:**
The system translates entire HTML sections (full documents) in one API call to maintain context. This is critical for consistency of academic terminology across paragraphs. Do not refactor to per-paragraph translation without understanding this design decision.

### Python Service Components

**html_parser.py (`LatinGrammarParser`):**
- Parses HTML using BeautifulSoup with lxml
- Classifies text segments into: `LATIN` (preserve), `ENGLISH` (translate), `GLOSS` (translate)
- Preserves formatting (bold, italic, tables, lists) via `FormattingStyle` enum
- Detection logic: CSS classes (`foreign`, `gloss`) and language detection for fallback

**models.py:**
- Pydantic models for type safety: `ParsedDocument`, `ParsedNode`, `TextSegment`, `TableStructure`
- `TextType` enum: LATIN, ENGLISH, GLOSS
- `NodeType` enum: PARAGRAPH, HEADING, LIST_ITEM, TABLE, etc.

**glossary.py:**
- 100+ technical grammar terms (Nominative→Nominativo, Conjugation→Conjugação, etc.)
- Injected into AI prompts to ensure consistent terminology
- Edit this file to add new term mappings

**translation_strategy.py (`SectionTranslator`):**
- Orchestrates full document translation
- Extracts all translatable segments with unique IDs
- Calls strategy.translate_section() with full context
- Applies translations back to ParsedDocument using segment ID mapping
- Includes retry logic (default: 3 attempts)

**gemini_translator.py / claude_translator.py:**
- Implement `translate_section()` method
- Build prompts with document context + glossary + structured segment list
- Parse JSON responses back into `TranslationResult`
- Handle API-specific error codes and rate limits

**html_generator.py (`HtmlGenerator`):**
- Reconstructs translated HTML from ParsedDocument
- Preserves document structure, CSS classes, and inline styles
- Handles text segment spacing intelligently
- Generates complete HTML with proper DOCTYPE and head section

**word_generator.py:**
- Temporary Word document generation using python-docx
- Final version will be in .NET with DocumentFormat.OpenXml
- Converts ParsedDocument to .docx preserving structure and formatting

### C# API Components

**Program.cs:**
- Minimal API with single `/process` endpoint
- Chains HtmlService.ParseHtml() → TranslationService.TranslateStructure()
- HttpClient configured to call Python service at `http://translator:5001`

**HtmlService.cs:**
- Uses AngleSharp for C# HTML parsing
- Currently simplified - main parsing logic is in Python service
- Returns structured node list

**TranslationService.cs:**
- HTTP client wrapper to Python `/translate` endpoint
- Serializes/deserializes JSON between services

## HTML Comments Handling

The parser ignores HTML comments (implemented in recent commit 1cc7f42). Comments in source HTML are completely skipped and not translated.

## Important Constraints

1. **Preserve Latin text exactly** - TextType.LATIN segments must never be translated
2. **Use glossary terms consistently** - Always check glossary.py before translating grammar terms
3. **Maintain document structure** - Node hierarchy and formatting must survive translation
4. **Section-level context** - Do not split translations into smaller units without preserving full document context
5. **API keys required** - System cannot function without GEMINI_API_KEY or CLAUDE_API_KEY environment variable

## Resource Files

Source HTML files are in `src/Resources/` (e.g., alphabet.htm, syntax.htm). These are the actual Allen & Greenough grammar chapters to be translated.

## Testing Strategy

1. **Parser tests** (`test_parser.py`) - Verify HTML parsing without AI calls
2. **Translation tests** (`test_translation.py`) - Full AI translation with configurable provider
3. **Full pipeline** (`test_full_pipeline.py`) - End-to-end: parse → translate → HTML + Word generation

Always test parser changes with `test_parser.py` first before testing translation (to avoid wasting API credits).

Use `--skip-translation` flag in test_full_pipeline.py to test HTML/Word generation without consuming API credits.

## Known Technical Debt

- Word generation currently in Python (python-docx), planned migration to .NET
- C# API parsing is simplified stub - main logic in Python
- No caching layer for translations (re-running wastes API credits)
- No batch processing optimization for multiple files
