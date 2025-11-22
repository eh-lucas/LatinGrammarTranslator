"""
API Flask para processamento e tradução da gramática latina
"""
import os
from flask import Flask, request, jsonify
from html_parser import LatinGrammarParser
from translator_factory import TranslatorFactory
from models import ParsedDocument

app = Flask(__name__)
parser = LatinGrammarParser()

# Configuração do tradutor via variáveis de ambiente
TRANSLATOR_PROVIDER = os.getenv("TRANSLATOR_PROVIDER", "gemini")
TRANSLATOR_API_KEY = os.getenv("TRANSLATOR_API_KEY", "")
TRANSLATOR_MODEL = os.getenv("TRANSLATOR_MODEL")  # None usa padrão do provider


@app.route("/parse-html", methods=["POST"])
def parse_html():
    """
    Endpoint para parsear HTML e retornar estrutura

    Request body:
    {
        "html": "<html>...</html>",
        "filename": "optional-filename.htm"
    }

    Response:
    {
        "title": "Document Title",
        "nodes": [...],
        "stats": {...}
    }
    """
    try:
        data = request.get_json()

        if not data or 'html' not in data:
            return jsonify({"error": "Campo 'html' é obrigatório"}), 400

        html_content = data['html']
        filename = data.get('filename')

        # Parsear HTML
        parsed_doc = parser.parse_html(html_content, filename)

        # Converter para dict (Pydantic model_dump)
        result = parsed_doc.model_dump()

        return jsonify(result), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route("/health", methods=["GET"])
def health():
    """Health check endpoint"""
    return jsonify({"status": "healthy", "service": "latin-grammar-parser"}), 200


@app.route("/translate", methods=["POST"])
def translate():
    """
    Endpoint para parsear e traduzir HTML ou ParsedDocument

    Request body (opção 1 - HTML):
    {
        "html": "<html>...</html>",
        "filename": "optional-filename.htm"
    }

    Request body (opção 2 - ParsedDocument já parseado):
    {
        "title": "...",
        "nodes": [...],
        ...
    }

    Response:
    {
        "title": "Document Title (traduzido)",
        "nodes": [...],  // com textos traduzidos
        "stats": {...}
    }
    """
    try:
        data = request.get_json()

        if not data:
            return jsonify({"error": "Request body é obrigatório"}), 400

        # Verificar se API key está configurada
        if not TRANSLATOR_API_KEY:
            return jsonify({
                "error": "TRANSLATOR_API_KEY não configurada. "
                        "Configure a variável de ambiente com sua API key."
            }), 500

        # Opção 1: Se recebeu HTML, parsear primeiro
        if 'html' in data:
            html_content = data['html']
            filename = data.get('filename')
            parsed_doc = parser.parse_html(html_content, filename)

        # Opção 2: Se recebeu ParsedDocument já parseado
        else:
            try:
                parsed_doc = ParsedDocument(**data)
            except Exception as e:
                return jsonify({
                    "error": f"Formato inválido. Esperado 'html' ou ParsedDocument válido. Erro: {str(e)}"
                }), 400

        # Criar tradutor
        try:
            translator = TranslatorFactory.create(
                provider=TRANSLATOR_PROVIDER,
                api_key=TRANSLATOR_API_KEY,
                model_name=TRANSLATOR_MODEL
            )
        except ValueError as e:
            return jsonify({"error": str(e)}), 500

        # Traduzir documento
        translated_doc = translator.translate_document(parsed_doc)

        # Converter para dict e retornar
        result = translated_doc.model_dump()
        return jsonify(result), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route("/stats", methods=["GET"])
def stats():
    """Retorna estatísticas do parser e tradutor"""
    return jsonify({
        "service": "Latin Grammar Parser & Translator",
        "version": "2.0.0",
        "translator": {
            "provider": TRANSLATOR_PROVIDER,
            "model": TRANSLATOR_MODEL or TranslatorFactory.get_default_models().get(TRANSLATOR_PROVIDER),
            "api_key_configured": bool(TRANSLATOR_API_KEY)
        },
        "endpoints": [
            "POST /parse-html - Parsear HTML e retornar estrutura",
            "POST /translate - Parsear e traduzir HTML ou ParsedDocument",
            "GET /health - Health check",
            "GET /stats - Estatísticas do serviço"
        ]
    }), 200


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001, debug=True)
