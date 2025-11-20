"""
Factory para criar instâncias de tradutores
"""
from typing import Optional, Dict
from translation_strategy import TranslationStrategy
from gemini_translator import GeminiTranslator
from claude_translator import ClaudeTranslator


class TranslatorFactory:
    """Factory para criar tradutores com Strategy pattern"""

    SUPPORTED_PROVIDERS = {
        "gemini": "Google Gemini (Grátis)",
        "claude": "Anthropic Claude ($5 grátis)",
    }

    @staticmethod
    def create(
        provider: str,
        api_key: str,
        model_name: Optional[str] = None,
        glossary: Optional[Dict[str, str]] = None
    ) -> TranslationStrategy:
        """
        Cria instância de tradutor baseado no provedor

        Args:
            provider: Nome do provedor ('gemini' ou 'claude')
            api_key: Chave da API
            model_name: Nome do modelo (opcional, usa padrão do provedor)
            glossary: Glossário customizado (opcional)

        Returns:
            Instância de TranslationStrategy

        Raises:
            ValueError: Se provedor não for suportado
        """
        provider = provider.lower()

        if provider == "gemini":
            model = model_name or "gemini-3-pro-preview"
            return GeminiTranslator(api_key, model, glossary)

        elif provider == "claude":
            model = model_name or "claude-3-5-sonnet-20241022"
            return ClaudeTranslator(api_key, model, glossary)

        else:
            raise ValueError(
                f"Provedor '{provider}' não suportado. "
                f"Opções: {', '.join(TranslatorFactory.SUPPORTED_PROVIDERS.keys())}"
            )

    @staticmethod
    def list_providers() -> Dict[str, str]:
        """
        Lista provedores suportados

        Returns:
            Dicionário {nome: descrição}
        """
        return TranslatorFactory.SUPPORTED_PROVIDERS.copy()

    @staticmethod
    def get_default_models() -> Dict[str, str]:
        """
        Retorna modelos padrão para cada provedor

        Returns:
            Dicionário {provedor: modelo_padrão}
        """
        return {
            "gemini": "gemini-3-pro-preview",
            "claude": "claude-3-5-sonnet-20241022"
        }

    @staticmethod
    def get_available_models(provider: str) -> list:
        """
        Lista modelos disponíveis para um provedor

        Args:
            provider: Nome do provedor

        Returns:
            Lista de modelos disponíveis
        """
        provider = provider.lower()

        if provider == "gemini":
            return [
                "gemini-3-pro-preview",  # Modelo mais recente e poderoso
                "gemini-1.5-flash",      # Mais rápido, grátis
                "gemini-1.5-pro",        # Mais poderoso (versão anterior)
                "gemini-2.0-flash-exp",  # Experimental
            ]
        elif provider == "claude":
            return [
                "claude-3-5-sonnet-20241022",  # Melhor custo-benefício
                "claude-3-opus-20240229",      # Mais poderoso
                "claude-3-haiku-20240307",     # Mais barato
            ]
        else:
            return []
