"""
Implementação de tradução usando Google Gemini API
"""
import google.generativeai as genai
import json
import re
from typing import Dict, Optional
from translation_strategy import TranslationStrategy, SectionData, TranslationResult


class GeminiTranslator(TranslationStrategy):
    """Tradutor usando Google Gemini API (Grátis!)"""

    def __init__(
        self,
        api_key: str,
        model_name: str = "gemini-1.5-flash",
        glossary: Optional[Dict[str, str]] = None
    ):
        """
        Inicializa tradutor Gemini

        Args:
            api_key: Chave da API Gemini
            model_name: Nome do modelo ('gemini-1.5-flash' ou 'gemini-1.5-pro')
            glossary: Glossário customizado (usa padrão se None)
        """
        super().__init__(api_key, glossary)

        # Configurar Gemini
        genai.configure(api_key=api_key)

        # Configuração de segurança (permite conteúdo educacional)
        self.safety_settings = [
            {
                "category": "HARM_CATEGORY_HARASSMENT",
                "threshold": "BLOCK_NONE"
            },
            {
                "category": "HARM_CATEGORY_HATE_SPEECH",
                "threshold": "BLOCK_NONE"
            },
            {
                "category": "HARM_CATEGORY_SEXUALLY_EXPLICIT",
                "threshold": "BLOCK_NONE"
            },
            {
                "category": "HARM_CATEGORY_DANGEROUS_CONTENT",
                "threshold": "BLOCK_NONE"
            }
        ]

        # Configuração de geração
        self.generation_config = {
            "temperature": 0.3,  # Baixa criatividade (mais consistente)
            "top_p": 0.95,
            "top_k": 40,
            "max_output_tokens": 8192,
        }

        self.model = genai.GenerativeModel(
            model_name=model_name,
            generation_config=self.generation_config,
            safety_settings=self.safety_settings
        )

        self.model_name = model_name

    def get_provider_name(self) -> str:
        return f"Google Gemini ({self.model_name})"

    def translate_section(self, section: SectionData) -> TranslationResult:
        """
        Traduz seção completa usando Gemini

        Args:
            section: Dados da seção

        Returns:
            Resultado da tradução
        """
        try:
            # Criar prompt
            prompt = self._create_section_prompt(section)

            print(f"[Gemini] Enviando {len(section.segments_to_translate)} segmentos...")
            print(f"[Gemini] Tamanho do prompt: ~{len(prompt)} caracteres")

            # Enviar para Gemini
            response = self.model.generate_content(prompt)

            # Extrair tokens usados
            tokens_used = None
            if hasattr(response, 'usage_metadata'):
                tokens_used = (
                    response.usage_metadata.prompt_token_count +
                    response.usage_metadata.candidates_token_count
                )
                print(f"[Gemini] Tokens usados: {tokens_used}")

            # Parsear resposta JSON
            translated_segments = self._parse_response(response.text)

            if not translated_segments:
                return TranslationResult(
                    success=False,
                    translated_segments={},
                    error_message="Falha ao parsear resposta JSON",
                    provider=self.get_provider_name()
                )

            print(f"[Gemini] Sucesso! {len(translated_segments)} segmentos traduzidos")

            return TranslationResult(
                success=True,
                translated_segments=translated_segments,
                tokens_used=tokens_used,
                provider=self.get_provider_name()
            )

        except Exception as e:
            error_msg = f"Erro no Gemini: {str(e)}"
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
