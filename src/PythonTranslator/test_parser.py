"""
Script de teste para o parser HTML
"""
import os
import sys
import json
from html_parser import LatinGrammarParser

# Fix encoding for Windows console
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')


def test_parser_with_file(filepath: str):
    """
    Testa o parser com um arquivo HTML real

    Args:
        filepath: Caminho para o arquivo HTML
    """
    print(f"\n{'='*80}")
    print(f"Testando parser com: {os.path.basename(filepath)}")
    print(f"{'='*80}\n")

    # Ler arquivo
    with open(filepath, 'r', encoding='utf-8') as f:
        html_content = f.read()

    print(f"[OK] Arquivo lido: {len(html_content)} caracteres\n")

    # Parsear
    parser = LatinGrammarParser()
    parsed_doc = parser.parse_html(html_content, os.path.basename(filepath))

    # Mostrar informações básicas
    print(f"Titulo: {parsed_doc.title}")
    print(f"Encoding: {parsed_doc.encoding}")
    print(f"CSS: {parsed_doc.css_file or 'N/A'}")
    print(f"Arquivo: {parsed_doc.original_filename}\n")

    # Estatísticas
    print("ESTATISTICAS:")
    print(f"  - Total de nos: {parsed_doc.stats['total_nodes']}")
    print(f"  - Segmentos de texto: {parsed_doc.stats['text_segments']}")
    print(f"  - Segmentos em latim: {parsed_doc.stats['latin_segments']}")
    print(f"  - Segmentos em ingles: {parsed_doc.stats['english_segments']}")
    print(f"  - Segmentos de gloss: {parsed_doc.stats['gloss_segments']}")
    print(f"  - Tabelas: {parsed_doc.stats['tables']}")
    print(f"  - Listas: {parsed_doc.stats['lists']}\n")

    # Mostrar primeiros nós
    print("PRIMEIROS NOS (ate 5):")
    for i, node in enumerate(parsed_doc.nodes[:5]):
        print(f"\n  [{i+1}] {node.node_type}")
        if node.node_id:
            print(f"      ID: {node.node_id}")
        if node.section_number:
            print(f"      Secao: {node.section_number}")

        # Mostrar segmentos de texto
        if node.text_segments:
            print(f"      Segmentos de texto: {len(node.text_segments)}")
            for j, segment in enumerate(node.text_segments[:3]):  # Max 3 por nó
                text_preview = segment.text[:50] + "..." if len(segment.text) > 50 else segment.text
                print(f"        [{j+1}] {segment.text_type}: \"{text_preview}\"")
                if segment.formatting.bold or segment.formatting.italic:
                    styles = []
                    if segment.formatting.bold:
                        styles.append("negrito")
                    if segment.formatting.italic:
                        styles.append("italico")
                    print(f"            Formatacao: {', '.join(styles)}")

        # Mostrar filhos
        if node.children:
            print(f"      Filhos: {len(node.children)}")

    print(f"\n{'='*80}")
    print("TESTE CONCLUIDO COM SUCESSO!")
    print(f"{'='*80}\n")

    return parsed_doc


def test_with_sample_html():
    """Testa com HTML de exemplo"""
    sample_html = """
    <!DOCTYPE html>
    <html>
    <head>
        <title>Test Document</title>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <link rel="stylesheet" href="site.css" />
    </head>
    <body class="simple">
        <h2>NOUN DECLENSIONS</h2>
        <p><strong>153.</strong> The inflection of the Verb is called its Conjugation.</p>
        <p>The word <span class="foreign" style="font-style: italic;">stella</span> means <span class="gloss" style="font-style: italic;">star</span>.</p>
        <p style="padding-left: 30px;"><strong><em>a.</em></strong> There are two Voices: Active and Passive.</p>
        <ul>
            <li>First declension</li>
            <li>Second declension</li>
        </ul>
    </body>
    </html>
    """

    print(f"\n{'='*80}")
    print("Testando parser com HTML de exemplo")
    print(f"{'='*80}\n")

    parser = LatinGrammarParser()
    parsed_doc = parser.parse_html(sample_html, "sample.htm")

    print(f"[OK] Parsing concluido")
    print(f"  - Nos: {len(parsed_doc.nodes)}")
    print(f"  - Segmentos de texto: {parsed_doc.stats['text_segments']}")
    print(f"  - Latim: {parsed_doc.stats['latin_segments']}")
    print(f"  - Ingles: {parsed_doc.stats['english_segments']}")

    # Mostrar estrutura
    print("\nESTRUTURA:")
    for i, node in enumerate(parsed_doc.nodes):
        indent = "  "
        print(f"{indent}[{i+1}] {node.node_type}")

        for segment in node.text_segments:
            text_preview = segment.text[:40] + "..." if len(segment.text) > 40 else segment.text
            print(f"{indent}  - {segment.text_type}: \"{text_preview}\"")

        for child in node.children:
            print(f"{indent}  +-- {child.node_type}")

    print(f"\n{'='*80}")
    print("TESTE DE EXEMPLO CONCLUIDO!")
    print(f"{'='*80}\n")


if __name__ == "__main__":
    # Primeiro, teste com HTML de exemplo
    test_with_sample_html()

    # Depois, teste com arquivo real se disponível
    resources_path = "../Resources"
    test_file = os.path.join(resources_path, "alphabet.htm")

    if os.path.exists(test_file):
        parsed = test_parser_with_file(test_file)

        # Opcionalmente, salvar resultado em JSON
        output_file = "parsed_output.json"
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(parsed.model_dump(), f, indent=2, ensure_ascii=False)
        print(f"[SAVED] Resultado salvo em: {output_file}")
    else:
        print(f"[AVISO] Arquivo nao encontrado: {test_file}")
        print(f"        Para testar com arquivo real, ajuste o caminho em test_parser.py")
