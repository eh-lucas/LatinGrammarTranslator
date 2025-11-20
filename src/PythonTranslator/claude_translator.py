"""
Implementação de tradução usando Anthropic Claude API
"""
import anthropic
import json
from typing import Dict, Optional
from translation_strategy import TranslationStrategy, SectionData, TranslationResult


class ClaudeTranslator(TranslationStrategy):
    """Tradutor usando Anthropic Claude API"""

    def __init__(
        self,
        api_key: str,
        model_name: str = "claude-3-5-sonnet-20241022",
        glossary: Optional[Dict[str, str]] = None
    ):
        """
        Inicializa tradutor Claude

        Args:
            api_key: Chave da API Claude
            model_name: Nome do modelo
            glossary: Glossário customizado (usa padrão se None)
        """
        super().__init__(api_key, glossary)

        self.client = anthropic.Anthropic(api_key=api_key)
        self.model_name = model_name

    def get_provider_name(self) -> str:
        return f"Anthropic Claude ({self.model_name})"

    def translate_section(self, section: SectionData) -> TranslationResult:
        """
        Traduz seção completa usando Claude

        Args:
            section: Dados da seção

        Returns:
            Resultado da tradução
        """
        try:
            # Criar prompt
            prompt = self._create_section_prompt(section)

            print(f"[Claude] Enviando {len(section.segments_to_translate)} segmentos...")
            print(f"[Claude] Tamanho do prompt: ~{len(prompt)} caracteres")

            # Enviar para Claude
            response = self.client.messages.create(
                model=self.model_name,
                max_tokens=8192,
                temperature=0.3,  # Baixa criatividade para consistência
                messages=[
                    {
                        "role": "user",
                        "content": prompt
                    }
                ]
            )

            # Extrair resposta
            response_text = response.content[0].text

            # Tokens usados
            tokens_used = response.usage.input_tokens + response.usage.output_tokens
            print(f"[Claude] Tokens usados: {tokens_used} (input: {response.usage.input_tokens}, output: {response.usage.output_tokens})")

            # Parsear resposta JSON
            translated_segments = self._parse_response(response_text)

            if not translated_segments:
                return TranslationResult(
                    success=False,
                    translated_segments={},
                    error_message="Falha ao parsear resposta JSON",
                    provider=self.get_provider_name()
                )

            print(f"[Claude] Sucesso! {len(translated_segments)} segmentos traduzidos")

            return TranslationResult(
                success=True,
                translated_segments=translated_segments,
                tokens_used=tokens_used,
                provider=self.get_provider_name()
            )

        except Exception as e:
            error_msg = f"Erro no Claude: {str(e)}"
            print(f"[ERRO] {error_msg}")

            return TranslationResult(
                success=False,
                translated_segments={},
                error_message=error_msg,
                provider=self.get_provider_name()
            )

    def _parse_response(self, response_text: str) -> Optional[Dict[str, str]]:
        """
        Parseia resposta JSON do modelo

        Args:
            response_text: Texto da resposta

        Returns:
            Dicionário {id: texto_traduzido} ou None se falhar
        """
        try:
            # Remover markdown code blocks se existirem
            cleaned = response_text.strip()

            # Remover ```json e ``` se presentes
            if cleaned.startswith("```"):
                # Encontrar primeiro { e último }
                start = cleaned.find("{")
                end = cleaned.rfind("}") + 1
                if start != -1 and end > start:
                    cleaned = cleaned[start:end]

            # Parsear JSON
            data = json.loads(cleaned)

            # Extrair traduções
            translations = {}
            if "translations" in data:
                for item in data["translations"]:
                    if "id" in item and "translated" in item:
                        translations[item["id"]] = item["translated"]

            return translations if translations else None

        except json.JSONDecodeError as e:
            print(f"[ERRO] Falha ao parsear JSON: {str(e)}")
            print(f"[DEBUG] Resposta recebida: {response_text[:500]}...")
            return None
        except Exception as e:
            print(f"[ERRO] Erro ao processar resposta: {str(e)}")
            return None
