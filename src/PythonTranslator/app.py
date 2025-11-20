"""
API Flask para processamento e tradução da gramática latina
"""
from flask import Flask, request, jsonify
from html_parser import LatinGrammarParser

app = Flask(__name__)
parser = LatinGrammarParser()


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


@app.route("/stats", methods=["GET"])
def stats():
    """Retorna estatísticas do parser"""
    return jsonify({
        "service": "Latin Grammar Parser",
        "version": "1.0.0",
        "endpoints": [
            "POST /parse-html - Parsear HTML e retornar estrutura",
            "GET /health - Health check",
            "GET /stats - Estatísticas do serviço"
        ]
    }), 200


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5001, debug=True)
