# 🌐 Latin Grammar Translator - Python Service

Sistema de tradução automática da gramática latina de Allen & Greenough usando IA.

## 📋 Funcionalidades

- ✅ **Parser HTML robusto** - Extrai estrutura completa dos documentos
- ✅ **Detecção inteligente** - Identifica latim vs inglês vs glosses
- ✅ **Tradução por seção** - Mantém contexto do documento inteiro
- ✅ **Múltiplos provedores** - Gemini (grátis) ou Claude ($5 grátis)
- ✅ **Glossário técnico** - 100+ termos gramaticais consistentes
- ✅ **Preservação de formatação** - Mantém negrito, itálico, tabelas, etc.

## 🚀 Quick Start

### 1. Instalar Dependências

```bash
pip install -r requirements.txt
```

### 2. Obter API Key (Gemini - Grátis!)

1. Acesse: https://aistudio.google.com/
2. Faça login com Google
3. Clique em "Get API Key"
4. Copie sua chave

### 3. Configurar API Key

**Windows:**
```cmd
set GEMINI_API_KEY=sua_chave_aqui
```

**Linux/Mac:**
```bash
export GEMINI_API_KEY=sua_chave_aqui
```

### 4. Testar Tradução

```bash
python test_translation.py
```

## 📖 Uso Detalhado

### Traduzir um arquivo específico

```bash
python test_translation.py ../Resources/alphabet.htm
```

### Usar Claude em vez de Gemini

```bash
# Configurar API key do Claude
set CLAUDE_API_KEY=sua_chave_claude

# Executar
python test_translation.py --provider claude
```

### Salvar resultado em JSON

```bash
python test_translation.py --output resultado.json
```

## 🏗️ Arquitetura

### Strategy Pattern

O sistema usa **Strategy Pattern** para suportar múltiplos provedores de tradução:

```
TranslationStrategy (interface)
    ├── GeminiTranslator
    └── ClaudeTranslator
```

### Fluxo de Tradução

```
1. Parser HTML → ParsedDocument
   ↓
2. SectionTranslator extrai segmentos
   ↓
3. Strategy cria prompt com contexto + glossário
   ↓
4. API de IA traduz seção completa
   ↓
5. Traduções aplicadas de volta ao documento
```

## 📁 Estrutura de Arquivos

```
PythonTranslator/
├── html_parser.py           # Parser HTML
├── models.py                # Modelos de dados (Pydantic)
├── glossary.py              # Glossário de termos técnicos
├── translation_strategy.py  # Interface Strategy + Orchestrator
├── gemini_translator.py     # Implementação Gemini
├── claude_translator.py     # Implementação Claude
├── translator_factory.py    # Factory para criar tradutores
├── test_translation.py      # Script de teste
├── test_parser.py           # Teste do parser
├── app.py                   # API Flask
└── requirements.txt         # Dependências
```

## 🔧 API Flask

### Endpoint: `/parse-html`

Parseia HTML e retorna estrutura.

```bash
curl -X POST http://localhost:5001/parse-html \
  -H "Content-Type: application/json" \
  -d '{"html": "<html>...</html>", "filename": "test.htm"}'
```

### Endpoint: `/health`

Health check.

```bash
curl http://localhost:5001/health
```

## 🎯 Provedores de IA

### Gemini (Google) ⭐ **RECOMENDADO PARA TESTES**

- ✅ **Totalmente gratuito**
- ✅ Sem cartão de crédito
- ✅ 1 milhão de tokens/dia
- ✅ 60 requests/minuto
- ⚠️ Qualidade boa mas não excepcional

**Como obter:**
1. https://aistudio.google.com/
2. Login com Google → Get API Key
3. Pronto!

### Claude (Anthropic) 💎 **MELHOR QUALIDADE**

- ✅ $5 USD grátis (30 dias)
- ✅ Excelente para textos acadêmicos
- ✅ Contexto gigante (200k tokens)
- ⚠️ Requer cartão de crédito
- 💰 ~$20-40 para projeto completo

**Como obter:**
1. https://console.anthropic.com/
2. Criar conta → Adicionar payment
3. Recebe $5 de crédito automaticamente

## 📊 Glossário de Termos

O sistema inclui glossário com 100+ termos técnicos:

```python
{
    "Nominative": "Nominativo",
    "Genitive": "Genitivo",
    "Declension": "Declinação",
    "Conjugation": "Conjugação",
    # ... mais 100+ termos
}
```

Editável em `glossary.py`.

## 🧪 Testes

### Testar Parser Apenas

```bash
python test_parser.py
```

### Testar Tradução com Arquivo Pequeno

```bash
python test_translation.py ../Resources/alphabet.htm
```

### Testar Tradução com Arquivo Grande

```bash
python test_translation.py ../Resources/syntax.htm --output syntax_traduzido.json
```

## 💡 Dicas

### 1. Começar com Gemini (Grátis)

```bash
# Teste todo o sistema sem gastar nada
export GEMINI_API_KEY=sua_chave
python test_translation.py
```

### 2. Comparar Qualidade

```bash
# Traduzir com Gemini
python test_translation.py --provider gemini --output gemini_result.json

# Traduzir com Claude
python test_translation.py --provider claude --output claude_result.json

# Comparar resultados
```

### 3. Processar Vários Arquivos

```bash
# Criar script batch
for file in ../Resources/*.htm; do
    python test_translation.py "$file" --output "translated/$(basename $file .htm).json"
done
```

## 🐛 Troubleshooting

### Erro: "API key não fornecida"

```bash
# Verificar se variável foi definida
echo %GEMINI_API_KEY%  # Windows
echo $GEMINI_API_KEY   # Linux/Mac

# Redefinir se necessário
set GEMINI_API_KEY=sua_chave
```

### Erro: "Falha ao parsear JSON"

A IA pode retornar formato incorreto. O sistema tenta 3x automaticamente.
Se persistir, tente outro modelo:

```bash
# Gemini Flash → Pro
python test_translation.py --provider gemini

# Ou use Claude
python test_translation.py --provider claude
```

### Erro: Rate Limit

Gemini Free: 60 req/min
- Solução: Adicionar delay entre arquivos

Claude: Depende do tier
- Solução: Upgrade ou usar Gemini

## 📈 Roadmap

- [ ] Suporte a GPT-4/GPT-4 Turbo
- [ ] Cache de traduções (evitar reprocessar)
- [ ] Interface web para revisão
- [ ] Geração de documento Word final
- [ ] Batch processing otimizado
- [ ] Métricas de qualidade

## 📝 Licença

Este projeto é parte do Latin Grammar Translator.

---

**Autor:** Sistema de Tradução IA
**Data:** 2025-11-20
