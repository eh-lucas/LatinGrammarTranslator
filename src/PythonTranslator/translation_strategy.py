"""
Strategy pattern para tradução por seção completa
Cada arquivo HTML é traduzido como uma seção inteira para manter contexto
"""
from abc import ABC, abstractmethod
from typing import List, Dict, Optional
from dataclasses import dataclass
from models import ParsedDocument, ParsedNode, TextSegment, TextType
from glossary import get_glossary, format_glossary_for_prompt
import json


@dataclass
class SectionData:
    """Dados de uma seção para tradução"""
    title: str
    filename: str
    segments_to_translate: List[Dict[str, any]]  # Lista de {id, text, type}
    total_segments: int
    latin_count: int
    english_count: int
    gloss_count: int


@dataclass
class TranslationResult:
    """Resultado da tradução de uma seção"""
    success: bool
    translated_segments: Dict[str, str]  # id -> texto traduzido
    error_message: Optional[str] = None
    tokens_used: Optional[int] = None
    provider: Optional[str] = None


class TranslationStrategy(ABC):
    """Interface abstrata para estratégias de tradução por seção"""

    def __init__(self, api_key: str, glossary: Optional[Dict[str, str]] = None):
        self.api_key = api_key
        self.glossary = glossary or get_glossary()
        self.stats = {
            "sections_translated": 0,
            "segments_translated": 0,
            "total_tokens": 0,
            "errors": 0
        }

    @abstractmethod
    def translate_section(self, section: SectionData) -> TranslationResult:
        """
        Traduz uma seção completa mantendo contexto

        Args:
            section: Dados da seção

        Returns:
            Resultado da tradução
        """
        pass

    @abstractmethod
    def get_provider_name(self) -> str:
        """Retorna nome do provedor (ex: 'Gemini', 'Claude')"""
        pass

    def get_stats(self) -> Dict:
        """Retorna estatísticas de uso"""
        return self.stats.copy()

    def reset_stats(self):
        """Reseta estatísticas"""
        self.stats = {
            "sections_translated": 0,
            "segments_translated": 0,
            "total_tokens": 0,
            "errors": 0
        }

    def _create_section_prompt(self, section: SectionData) -> str:
        """
        Cria prompt para traduzir seção completa

        Args:
            section: Dados da seção

        Returns:
            Prompt formatado
        """
        glossary_text = format_glossary_for_prompt(self.glossary)

        # Montar lista de segmentos em formato estruturado
        segments_json = []
        for seg in section.segments_to_translate:
            segments_json.append({
                "id": seg["id"],
                "text": seg["text"],
                "type": seg["type"]
            })

        segments_text = json.dumps(segments_json, indent=2, ensure_ascii=False)

        prompt = f"""Você é um tradutor especializado em textos acadêmicos de gramática latina do livro "New Latin Grammar" de Allen & Greenough.

Sua tarefa é traduzir uma seção completa do inglês para português brasileiro, mantendo:
- Precisão terminológica
- Tom acadêmico e formal
- Consistência ao longo da seção

═══════════════════════════════════════════════════════════════════════
INFORMAÇÕES DA SEÇÃO:
═══════════════════════════════════════════════════════════════════════
Título: {section.title}
Arquivo: {section.filename}
Total de segmentos para traduzir: {len(section.segments_to_translate)}

═══════════════════════════════════════════════════════════════════════
GLOSSÁRIO DE TERMOS TÉCNICOS (use SEMPRE que aplicável):
═══════════════════════════════════════════════════════════════════════
{glossary_text}

═══════════════════════════════════════════════════════════════════════
REGRAS CRÍTICAS:
═══════════════════════════════════════════════════════════════════════
1. Use APENAS termos do glossário para conceitos gramaticais
2. Mantenha consistência terminológica em toda a seção
3. Preserve números, referências (§ 153, etc) e formatação
4. NÃO invente traduções - use o glossário
5. Se um segmento for tipo "gloss", traduza de forma natural (é tradução de exemplo latino)
6. Se um segmento for tipo "english", traduza mantendo tom explicativo acadêmico
7. Retorne EXATAMENTE no formato JSON especificado abaixo

═══════════════════════════════════════════════════════════════════════
SEGMENTOS PARA TRADUZIR:
═══════════════════════════════════════════════════════════════════════
{segments_text}

═══════════════════════════════════════════════════════════════════════
FORMATO DE RESPOSTA OBRIGATÓRIO:
═══════════════════════════════════════════════════════════════════════
Retorne um objeto JSON com a seguinte estrutura:
{{
  "translations": [
    {{
      "id": "id_do_segmento",
      "translated": "texto traduzido aqui"
    }},
    ...
  ]
}}

IMPORTANTE: Retorne APENAS o JSON, sem texto adicional antes ou depois.
"""

        return prompt


class SectionTranslator:
    """
    Orquestrador que traduz documentos por seção completa
    """

    def __init__(self, strategy: TranslationStrategy, max_retries: int = 3):
        self.strategy = strategy
        self.max_retries = max_retries

    def translate_document(self, parsed_doc: ParsedDocument) -> ParsedDocument:
        """
        Traduz documento completo

        Args:
            parsed_doc: Documento parseado

        Returns:
            Documento traduzido
        """
        print(f"\n{'='*80}")
        print(f"TRADUÇÃO POR SEÇÃO COMPLETA")
        print(f"Provedor: {self.strategy.get_provider_name()}")
        print(f"Arquivo: {parsed_doc.original_filename}")
        print(f"{'='*80}\n")

        # Extrair todos os segmentos que precisam tradução
        section = self._extract_section_data(parsed_doc)

        print(f"Seção: {section.title}")
        print(f"  - Total de segmentos: {section.total_segments}")
        print(f"  - Latim (preservar): {section.latin_count}")
        print(f"  - Inglês (traduzir): {section.english_count}")
        print(f"  - Gloss (traduzir): {section.gloss_count}")
        print(f"  - Para traduzir: {len(section.segments_to_translate)}\n")

        # Traduzir seção completa
        result = self._translate_with_retry(section)

        if result.success:
            # Aplicar traduções de volta ao documento
            self._apply_translations(parsed_doc, result.translated_segments)

            print(f"[OK] Seção traduzida com sucesso!")
            print(f"  - Segmentos traduzidos: {len(result.translated_segments)}")
            if result.tokens_used:
                print(f"  - Tokens usados: {result.tokens_used}")
        else:
            print(f"[ERRO] Falha na tradução: {result.error_message}")

        # Estatísticas finais
        stats = self.strategy.get_stats()
        print(f"\n{'='*80}")
        print(f"ESTATÍSTICAS:")
        print(f"  - Seções traduzidas: {stats['sections_translated']}")
        print(f"  - Segmentos traduzidos: {stats['segments_translated']}")
        print(f"  - Tokens totais: {stats['total_tokens']}")
        print(f"  - Erros: {stats['errors']}")
        print(f"{'='*80}\n")

        return parsed_doc

    def _extract_section_data(self, parsed_doc: ParsedDocument) -> SectionData:
        """Extrai dados da seção para tradução"""

        segments_to_translate = []
        segment_id = 0
        latin_count = 0
        english_count = 0
        gloss_count = 0

        def extract_recursive(node: ParsedNode):
            nonlocal segment_id, latin_count, english_count, gloss_count

            for segment in node.text_segments:
                if segment.text_type == TextType.LATIN:
                    latin_count += 1
                elif segment.text_type == TextType.ENGLISH:
                    english_count += 1
                    segments_to_translate.append({
                        "id": f"seg_{segment_id}",
                        "text": segment.text,
                        "type": "english",
                        "segment_ref": segment  # Guardar referência
                    })
                    segment_id += 1
                elif segment.text_type == TextType.GLOSS:
                    gloss_count += 1
                    segments_to_translate.append({
                        "id": f"seg_{segment_id}",
                        "text": segment.text,
                        "type": "gloss",
                        "segment_ref": segment
                    })
                    segment_id += 1

            for child in node.children:
                extract_recursive(child)

        for node in parsed_doc.nodes:
            extract_recursive(node)

        return SectionData(
            title=parsed_doc.title,
            filename=parsed_doc.original_filename or "unknown",
            segments_to_translate=segments_to_translate,
            total_segments=segment_id + latin_count,
            latin_count=latin_count,
            english_count=english_count,
            gloss_count=gloss_count
        )

    def _translate_with_retry(self, section: SectionData) -> TranslationResult:
        """Traduz com retry em caso de erro"""

        for attempt in range(self.max_retries):
            try:
                print(f"[Tentativa {attempt + 1}/{self.max_retries}] Enviando seção para tradução...")

                result = self.strategy.translate_section(section)

                if result.success:
                    self.strategy.stats['sections_translated'] += 1
                    self.strategy.stats['segments_translated'] += len(result.translated_segments)
                    if result.tokens_used:
                        self.strategy.stats['total_tokens'] += result.tokens_used
                    return result
                else:
                    print(f"[AVISO] Tentativa {attempt + 1} retornou erro: {result.error_message}")
                    if attempt < self.max_retries - 1:
                        print(f"[RETRY] Tentando novamente...")
                        continue

            except Exception as e:
                print(f"[ERRO] Tentativa {attempt + 1} falhou com exceção: {str(e)}")
                if attempt < self.max_retries - 1:
                    print(f"[RETRY] Tentando novamente...")
                    continue

        # Todas as tentativas falharam
        self.strategy.stats['errors'] += 1
        return TranslationResult(
            success=False,
            translated_segments={},
            error_message="Todas as tentativas falharam"
        )

    def _apply_translations(self, parsed_doc: ParsedDocument, translations: Dict[str, str]):
        """Aplica traduções de volta aos segmentos do documento"""

        segment_map = {}

        # Criar mapeamento id -> segmento
        def map_recursive(node: ParsedNode):
            segment_id = 0
            for segment in node.text_segments:
                if segment.text_type in [TextType.ENGLISH, TextType.GLOSS]:
                    segment_map[f"seg_{segment_id}"] = segment
                    segment_id += 1

            for child in node.children:
                map_recursive(child)

        # Mapear todos os segmentos
        segment_id_counter = 0
        for node in parsed_doc.nodes:
            for segment in node.text_segments:
                if segment.text_type in [TextType.ENGLISH, TextType.GLOSS]:
                    segment_id = f"seg_{segment_id_counter}"
                    if segment_id in translations:
                        segment.text = translations[segment_id]
                    segment_id_counter += 1

            def apply_recursive(node: ParsedNode):
                nonlocal segment_id_counter
                for child in node.children:
                    for segment in child.text_segments:
                        if segment.text_type in [TextType.ENGLISH, TextType.GLOSS]:
                            segment_id = f"seg_{segment_id_counter}"
                            if segment_id in translations:
                                segment.text = translations[segment_id]
                            segment_id_counter += 1
                    apply_recursive(child)

            apply_recursive(node)
