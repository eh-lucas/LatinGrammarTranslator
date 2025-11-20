"""
Script de teste para tradução com AI
"""
import os
import sys
import json
from html_parser import LatinGrammarParser
from translator_factory import TranslatorFactory
from translation_strategy import SectionTranslator

# Fix encoding para Windows
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')


def test_translation(
    html_file: str,
    provider: str = "gemini",
    api_key: str = None,
    output_file: str = None
):
    """
    Testa tradução de um arquivo HTML

    Args:
        html_file: Caminho para arquivo HTML
        provider: Provedor de IA ('gemini' ou 'claude')
        api_key: Chave da API (usa variável de ambiente se None)
        output_file: Arquivo de saída (opcional)
    """
    print(f"\n{'='*80}")
    print(f"TESTE DE TRADUÇÃO")
    print(f"{'='*80}\n")

    # Verificar API key
    if not api_key:
        env_var = f"{provider.upper()}_API_KEY"
        api_key = os.getenv(env_var)
        if not api_key:
            print(f"[ERRO] API key não fornecida!")
            print(f"       Defina a variável de ambiente {env_var}")
            print(f"       Ou passe como parâmetro: --api-key SUA_CHAVE")
            return

    # Verificar arquivo
    if not os.path.exists(html_file):
        print(f"[ERRO] Arquivo não encontrado: {html_file}")
        return

    print(f"Arquivo: {html_file}")
    print(f"Provedor: {provider}")
    print(f"API Key: {'*' * (len(api_key) - 4) + api_key[-4:]}\n")

    # Passo 1: Parsear HTML
    print(f"{'─'*80}")
    print(f"PASSO 1: Parseando HTML...")
    print(f"{'─'*80}\n")

    with open(html_file, 'r', encoding='utf-8') as f:
        html_content = f.read()

    parser = LatinGrammarParser()
    parsed_doc = parser.parse_html(html_content, os.path.basename(html_file))

    print(f"[OK] Documento parseado")
    print(f"  - Título: {parsed_doc.title}")
    print(f"  - Total de nós: {parsed_doc.stats['total_nodes']}")
    print(f"  - Segmentos de texto: {parsed_doc.stats['text_segments']}")
    print(f"  - Latim (preservar): {parsed_doc.stats['latin_segments']}")
    print(f"  - Inglês (traduzir): {parsed_doc.stats['english_segments']}")
    print(f"  - Gloss (traduzir): {parsed_doc.stats['gloss_segments']}\n")

    # Passo 2: Criar tradutor
    print(f"{'─'*80}")
    print(f"PASSO 2: Criando tradutor...")
    print(f"{'─'*80}\n")

    try:
        strategy = TranslatorFactory.create(provider, api_key)
        translator = SectionTranslator(strategy, max_retries=3)
        print(f"[OK] Tradutor criado: {strategy.get_provider_name()}\n")
    except ValueError as e:
        print(f"[ERRO] {str(e)}")
        print(f"\nProvedores disponíveis:")
        for name, desc in TranslatorFactory.list_providers().items():
            print(f"  - {name}: {desc}")
        return

    # Passo 3: Traduzir documento
    print(f"{'─'*80}")
    print(f"PASSO 3: Traduzindo documento...")
    print(f"{'─'*80}\n")

    translated_doc = translator.translate_document(parsed_doc)

    # Passo 4: Mostrar exemplos
    print(f"{'─'*80}")
    print(f"PASSO 4: Verificando traduções...")
    print(f"{'─'*80}\n")

    show_translation_samples(translated_doc, max_samples=5)

    # Passo 5: Salvar resultado
    if output_file:
        print(f"{'─'*80}")
        print(f"PASSO 5: Salvando resultado...")
        print(f"{'─'*80}\n")

        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(translated_doc.model_dump(), f, indent=2, ensure_ascii=False)

        print(f"[OK] Resultado salvo em: {output_file}\n")

    print(f"{'='*80}")
    print(f"TESTE CONCLUÍDO COM SUCESSO!")
    print(f"{'='*80}\n")


def show_translation_samples(parsed_doc, max_samples=5):
    """Mostra exemplos de traduções"""

    print("Exemplos de traduções:\n")

    count = 0
    for node in parsed_doc.nodes:
        if count >= max_samples:
            break

        for segment in node.text_segments:
            if count >= max_samples:
                break

            if segment.text_type.value in ["english", "gloss"]:
                print(f"[{count + 1}] Tipo: {segment.text_type.value}")
                print(f"    Texto: {segment.text[:100]}{'...' if len(segment.text) > 100 else ''}")

                if segment.formatting.bold:
                    print(f"    Formatação: NEGRITO")
                if segment.formatting.italic:
                    print(f"    Formatação: ITÁLICO")

                print()
                count += 1

        # Verificar filhos recursivamente
        def check_children(node):
            nonlocal count
            for child in node.children:
                if count >= max_samples:
                    return
                for segment in child.text_segments:
                    if count >= max_samples:
                        return
                    if segment.text_type.value in ["english", "gloss"]:
                        print(f"[{count + 1}] Tipo: {segment.text_type.value}")
                        print(f"    Texto: {segment.text[:100]}{'...' if len(segment.text) > 100 else ''}")
                        print()
                        count += 1
                check_children(child)

        check_children(node)

    if count == 0:
        print("[AVISO] Nenhum segmento traduzível encontrado")


def main():
    """Função principal"""
    import argparse

    parser = argparse.ArgumentParser(description="Teste de tradução com AI")
    parser.add_argument(
        "html_file",
        nargs="?",
        default="../Resources/alphabet.htm",
        help="Arquivo HTML para traduzir (padrão: alphabet.htm)"
    )
    parser.add_argument(
        "--provider",
        choices=["gemini", "claude"],
        default="gemini",
        help="Provedor de IA (padrão: gemini)"
    )
    parser.add_argument(
        "--api-key",
        help="Chave da API (ou use variável de ambiente)"
    )
    parser.add_argument(
        "--output",
        help="Arquivo de saída JSON (opcional)"
    )

    args = parser.parse_args()

    # Executar teste
    test_translation(
        html_file=args.html_file,
        provider=args.provider,
        api_key=args.api_key,
        output_file=args.output
    )


if __name__ == "__main__":
    # Se não tiver argumentos, mostrar ajuda e usar padrões
    if len(sys.argv) == 1:
        print("\n" + "="*80)
        print("TESTE DE TRADUÇÃO - Latin Grammar Translator")
        print("="*80)
        print("\nModo de uso:")
        print("  python test_translation.py [arquivo.htm] --provider gemini --api-key SUA_CHAVE")
        print("\nOu defina variável de ambiente:")
        print("  Windows: set GEMINI_API_KEY=sua_chave")
        print("  Linux:   export GEMINI_API_KEY=sua_chave")
        print("\nDepois execute:")
        print("  python test_translation.py")
        print("\n" + "="*80 + "\n")

        # Verificar se há API key em variável de ambiente
        if os.getenv("GEMINI_API_KEY"):
            print("[INFO] Detectada GEMINI_API_KEY. Executando teste com alphabet.htm...\n")
            test_translation(
                html_file="../Resources/alphabet.htm",
                provider="gemini"
            )
        else:
            print("[AVISO] Nenhuma API key encontrada.")
            print("        Defina GEMINI_API_KEY ou CLAUDE_API_KEY para continuar.\n")
    else:
        main()
