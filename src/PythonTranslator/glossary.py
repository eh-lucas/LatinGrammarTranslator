"""
Glossário de termos técnicos de gramática latina
Inglês → Português Brasileiro
"""

# Glossário completo de termos gramaticais
GRAMMAR_GLOSSARY = {
    # Casos
    "Nominative": "Nominativo",
    "Genitive": "Genitivo",
    "Dative": "Dativo",
    "Accusative": "Acusativo",
    "Ablative": "Ablativo",
    "Vocative": "Vocativo",
    "Locative": "Locativo",

    # Números
    "Singular": "Singular",
    "Plural": "Plural",
    "Dual": "Dual",

    # Gêneros
    "Masculine": "Masculino",
    "Feminine": "Feminino",
    "Neuter": "Neutro",
    "Common": "Comum",

    # Declinações
    "Declension": "Declinação",
    "First Declension": "Primeira Declinação",
    "Second Declension": "Segunda Declinação",
    "Third Declension": "Terceira Declinação",
    "Fourth Declension": "Quarta Declinação",
    "Fifth Declension": "Quinta Declinação",

    # Conjugações
    "Conjugation": "Conjugação",
    "First Conjugation": "Primeira Conjugação",
    "Second Conjugation": "Segunda Conjugação",
    "Third Conjugation": "Terceira Conjugação",
    "Fourth Conjugation": "Quarta Conjugação",

    # Vozes
    "Voice": "Voz",
    "Active": "Ativa",
    "Passive": "Passiva",
    "Middle": "Média",
    "Deponent": "Depoente",

    # Modos
    "Mood": "Modo",
    "Indicative": "Indicativo",
    "Subjunctive": "Subjuntivo",
    "Imperative": "Imperativo",
    "Infinitive": "Infinitivo",

    # Tempos
    "Tense": "Tempo",
    "Present": "Presente",
    "Imperfect": "Imperfeito",
    "Future": "Futuro",
    "Perfect": "Perfeito",
    "Pluperfect": "Mais-que-perfeito",
    "Future Perfect": "Futuro Perfeito",

    # Pessoas
    "Person": "Pessoa",
    "First Person": "Primeira Pessoa",
    "Second Person": "Segunda Pessoa",
    "Third Person": "Terceira Pessoa",

    # Classes de palavras
    "Noun": "Substantivo",
    "Pronoun": "Pronome",
    "Adjective": "Adjetivo",
    "Verb": "Verbo",
    "Adverb": "Advérbio",
    "Preposition": "Preposição",
    "Conjunction": "Conjunção",
    "Interjection": "Interjeição",
    "Particle": "Partícula",

    # Pronomes
    "Personal Pronoun": "Pronome Pessoal",
    "Possessive": "Possessivo",
    "Demonstrative": "Demonstrativo",
    "Relative": "Relativo",
    "Interrogative": "Interrogativo",
    "Indefinite": "Indefinido",
    "Reflexive": "Reflexivo",

    # Particípios e formas nominais
    "Participle": "Particípio",
    "Present Participle": "Particípio Presente",
    "Past Participle": "Particípio Passado",
    "Future Participle": "Particípio Futuro",
    "Gerund": "Gerúndio",
    "Gerundive": "Gerundivo",
    "Supine": "Supino",

    # Graus
    "Positive": "Positivo",
    "Comparative": "Comparativo",
    "Superlative": "Superlativo",

    # Sintaxe
    "Subject": "Sujeito",
    "Predicate": "Predicado",
    "Object": "Objeto",
    "Direct Object": "Objeto Direto",
    "Indirect Object": "Objeto Indireto",
    "Complement": "Complemento",
    "Attribute": "Atributo",
    "Apposition": "Aposição",

    # Orações
    "Clause": "Oração",
    "Main Clause": "Oração Principal",
    "Subordinate Clause": "Oração Subordinada",
    "Relative Clause": "Oração Relativa",
    "Conditional": "Condicional",
    "Temporal": "Temporal",
    "Causal": "Causal",
    "Final": "Final",
    "Consecutive": "Consecutiva",
    "Concessive": "Concessiva",

    # Discurso
    "Direct Discourse": "Discurso Direto",
    "Indirect Discourse": "Discurso Indireto",

    # Morfologia
    "Inflection": "Flexão",
    "Stem": "Radical",
    "Root": "Raiz",
    "Ending": "Desinência",
    "Suffix": "Sufixo",
    "Prefix": "Prefixo",
    "Infix": "Infixo",

    # Fonética
    "Vowel": "Vogal",
    "Consonant": "Consoante",
    "Diphthong": "Ditongo",
    "Syllable": "Sílaba",
    "Accent": "Acento",
    "Long": "Longa",
    "Short": "Breve",
    "Quantity": "Quantidade",

    # Outros
    "Gender": "Gênero",
    "Number": "Número",
    "Case": "Caso",
    "Syntax": "Sintaxe",
    "Grammar": "Gramática",
    "Word Formation": "Formação de Palavras",
    "Etymology": "Etimologia",
    "Orthography": "Ortografia",
    "Pronunciation": "Pronúncia",
    "Alphabet": "Alfabeto",
}


def get_glossary() -> dict:
    """Retorna o glossário completo"""
    return GRAMMAR_GLOSSARY.copy()


def format_glossary_for_prompt(glossary: dict = None, max_terms: int = None) -> str:
    """
    Formata glossário para inclusão em prompt

    Args:
        glossary: Glossário customizado (usa padrão se None)
        max_terms: Limite de termos (todos se None)

    Returns:
        String formatada para prompt
    """
    if glossary is None:
        glossary = GRAMMAR_GLOSSARY

    items = list(glossary.items())
    if max_terms:
        items = items[:max_terms]

    formatted = "\n".join([f"  • {en} → {pt}" for en, pt in items])
    return formatted


def search_glossary(term: str, glossary: dict = None) -> str:
    """
    Busca termo no glossário (case-insensitive)

    Args:
        term: Termo em inglês
        glossary: Glossário customizado

    Returns:
        Tradução em português ou termo original se não encontrado
    """
    if glossary is None:
        glossary = GRAMMAR_GLOSSARY

    # Busca exata
    if term in glossary:
        return glossary[term]

    # Busca case-insensitive
    term_lower = term.lower()
    for key, value in glossary.items():
        if key.lower() == term_lower:
            return value

    return term  # Não encontrado, retorna original
