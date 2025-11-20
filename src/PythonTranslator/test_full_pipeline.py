"""
Teste do pipeline completo: HTML → Parser → Tradução → Word
"""
import os
import sys
from html_parser import LatinGrammarParser
from translator_factory import TranslatorFactory
from translation_strategy import SectionTranslator
from word_generator import SimpleWordGenerator

# Fix encoding para Windows
if sys.platform == 'win32':
    sys.stdout.reconfigure(encoding='utf-8')


def full_pipeline_test(
    html_file: str,
    output_word: str,
    provider: str = "gemini",
    api_key: str = None,
    skip_translation: bool = False
):
    """
    Pipeline completo: HTML → Parser → Tradução → Word

    Args:
        html_file: Arquivo HTML de entrada
        output_word: Arquivo Word de saída (.docx)
        provider: Provedor de tradução ('gemini' ou 'claude')
        api_key: Chave da API (ou usa variável de ambiente)
        skip_translation: Se True, pula tradução (apenas testa parser + Word)
    """
    print(f"\n{'='*80}")
    print(f"PIPELINE COMPLETO - Latin Grammar Translator")
    print(f"{'='*80}\n")

    # Verificar arquivos
    if not os.path.exists(html_file):
        print(f"[ERRO] Arquivo não encontrado: {html_file}")
        return

    print(f"Entrada: {html_file}")
    print(f"Saída: {output_word}")
    print(f"Tradução: {'SIM' if not skip_translation else 'NÃO (apenas teste)'}\n")

    # ========================================================================
    # PASSO 1: PARSEAR HTML
    # ========================================================================
    print(f"{'─'*80}")
    print(f"PASSO 1: Parseando HTML...")
    print(f"{'─'*80}\n")

    with open(html_file, 'r', encoding='utf-8') as f:
        html_content = f.read()

    parser = LatinGrammarParser()
    parsed_doc = parser.parse_html(html_content, os.path.basename(html_file))

    print(f"[OK] HTML parseado com sucesso")
    print(f"  - Título: {parsed_doc.title}")
    print(f"  - Nós: {parsed_doc.stats['total_nodes']}")
    print(f"  - Segmentos: {parsed_doc.stats['text_segments']}")
    print(f"  - Latim: {parsed_doc.stats['latin_segments']}")
    print(f"  - Inglês: {parsed_doc.stats['english_segments']}")
    print(f"  - Gloss: {parsed_doc.stats['gloss_segments']}\n")

    # ========================================================================
    # PASSO 2: TRADUZIR (se não for skip)
    # ========================================================================
    if not skip_translation:
        print(f"{'─'*80}")
        print(f"PASSO 2: Traduzindo com IA...")
        print(f"{'─'*80}\n")

        # Verificar API key
        if not api_key:
            env_var = f"{provider.upper()}_API_KEY"
            api_key = os.getenv(env_var)
            if not api_key:
                print(f"[ERRO] API key não fornecida!")
                print(f"       Defina {env_var} ou use --skip-translation\n")
                return

        try:
            # Criar tradutor
            strategy = TranslatorFactory.create(provider, api_key)
            translator = SectionTranslator(strategy, max_retries=3)

            # Traduzir
            parsed_doc = translator.translate_document(parsed_doc)

            print(f"[OK] Tradução concluída\n")

        except Exception as e:
            print(f"[ERRO] Falha na tradução: {str(e)}")
            print(f"       Use --skip-translation para testar sem traduzir\n")
            return
    else:
        print(f"{'─'*80}")
        print(f"PASSO 2: Pulando tradução (modo teste)")
        print(f"{'─'*80}\n")

    # ========================================================================
    # PASSO 3: GERAR WORD
    # ========================================================================
    print(f"{'─'*80}")
    print(f"PASSO 3: Gerando documento Word...")
    print(f"{'─'*80}\n")

    try:
        generator = SimpleWordGenerator()
        generator.generate_from_parsed(parsed_doc, output_word)

        print(f"[OK] Documento Word gerado com sucesso!")
        print(f"     Arquivo: {output_word}\n")

    except Exception as e:
        print(f"[ERRO] Falha ao gerar Word: {str(e)}\n")
        import traceback
        traceback.print_exc()
        return

    # ========================================================================
    # RESUMO FINAL
    # ========================================================================
    print(f"{'='*80}")
    print(f"PIPELINE CONCLUÍDO COM SUCESSO!")
    print(f"{'='*80}")
    print(f"\nResumo:")
    print(f"  1. HTML parseado: {parsed_doc.stats['total_nodes']} nós")
    if not skip_translation:
        print(f"  2. Tradução: {parsed_doc.stats['english_segments'] + parsed_doc.stats['gloss_segments']} segmentos")
    else:
        print(f"  2. Tradução: PULADA (modo teste)")
    print(f"  3. Word gerado: {output_word}")
    print(f"\nAbra o arquivo Word para ver o resultado!")
    print(f"{'='*80}\n")


def main():
    """Função principal"""
    import argparse

    parser = argparse.ArgumentParser(description="Pipeline completo de tradução")
    parser.add_argument(
        "html_file",
        nargs="?",
        default="../Resources/alphabet.htm",
        help="Arquivo HTML para processar"
    )
    parser.add_argument(
        "--output",
        default="output.docx",
        help="Arquivo Word de saída (padrão: output.docx)"
    )
    parser.add_argument(
        "--provider",
        choices=["gemini", "claude"],
        default="gemini",
        help="Provedor de IA para tradução"
    )
    parser.add_argument(
        "--api-key",
        help="Chave da API (ou use variável de ambiente)"
    )
    parser.add_argument(
        "--skip-translation",
        action="store_true",
        help="Pular tradução (apenas testar parser + Word)"
    )

    args = parser.parse_args()

    # Executar pipeline
    full_pipeline_test(
        html_file=args.html_file,
        output_word=args.output,
        provider=args.provider,
        api_key=args.api_key,
        skip_translation=args.skip_translation
    )


if __name__ == "__main__":
    # Se não tiver argumentos, mostrar ajuda
    if len(sys.argv) == 1:
        print("\n" + "="*80)
        print("PIPELINE COMPLETO - Latin Grammar Translator")
        print("="*80)
        print("\nModo de uso:")
        print("\n1. Teste rápido SEM tradução (apenas parser + Word):")
        print("   python test_full_pipeline.py --skip-translation")
        print("\n2. Pipeline completo COM tradução:")
        print("   python test_full_pipeline.py --output resultado.docx")
        print("\n3. Arquivo específico:")
        print("   python test_full_pipeline.py ../Resources/inflection.htm --output inflection.docx")
        print("\nVariáveis de ambiente:")
        print("  Windows: set GEMINI_API_KEY=sua_chave")
        print("  Linux:   export GEMINI_API_KEY=sua_chave")
        print("\n" + "="*80 + "\n")

        # Tentar executar teste sem tradução
        if os.path.exists("../Resources/alphabet.htm"):
            print("[INFO] Executando teste rápido sem tradução...\n")
            full_pipeline_test(
                html_file="../Resources/alphabet.htm",
                output_word="test_output.docx",
                skip_translation=True
            )
        else:
            print("[AVISO] Arquivo alphabet.htm não encontrado.")
            print("        Execute a partir da pasta PythonTranslator/\n")
    else:
        main()
