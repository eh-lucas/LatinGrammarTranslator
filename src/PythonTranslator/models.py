"""
Modelos de dados para representar a estrutura HTML parseada
"""
from typing import List, Optional, Dict, Any
from pydantic import BaseModel
from enum import Enum


class NodeType(str, Enum):
    """Tipos de nós na estrutura do documento"""
    HEADING_1 = "h1"
    HEADING_2 = "h2"
    HEADING_3 = "h3"
    HEADING_4 = "h4"
    PARAGRAPH = "p"
    LIST_ORDERED = "ol"
    LIST_UNORDERED = "ul"
    LIST_ITEM = "li"
    TABLE = "table"
    TABLE_ROW = "tr"
    TABLE_CELL = "td"
    TABLE_HEADER = "th"
    BLOCKQUOTE = "blockquote"
    DIV = "div"
    SPAN = "span"
    LINK = "a"
    STRONG = "strong"
    EMPHASIS = "em"


class TextType(str, Enum):
    """Tipos de texto identificados"""
    ENGLISH = "english"        # Texto em inglês (deve ser traduzido)
    LATIN = "latin"            # Texto em latim (deve ser preservado)
    GLOSS = "gloss"            # Tradução/gloss (deve ser traduzido)
    REFERENCE = "reference"    # Referência bibliográfica (preservar)
    MIXED = "mixed"            # Conteúdo misto


class FormattingStyle(BaseModel):
    """Estilo de formatação aplicado ao texto"""
    bold: bool = False
    italic: bool = False
    underline: bool = False
    font_size: Optional[str] = None
    font_family: Optional[str] = None
    color: Optional[str] = None
    padding_left: Optional[str] = None
    text_align: Optional[str] = None

    class Config:
        extra = "allow"  # Permite campos adicionais


class TextSegment(BaseModel):
    """Segmento de texto com tipo e formatação"""
    text: str
    text_type: TextType
    formatting: FormattingStyle = FormattingStyle()
    html_class: Optional[str] = None  # Classe CSS original (ex: "foreign", "gloss")


class ParsedNode(BaseModel):
    """Nó parseado da estrutura HTML"""
    node_type: NodeType
    node_id: Optional[str] = None  # ID do elemento HTML (para referências)

    # Conteúdo textual estruturado
    text_segments: List[TextSegment] = []

    # Atributos HTML preservados
    attributes: Dict[str, str] = {}

    # Formatação inline (style attribute)
    inline_style: Optional[str] = None

    # Hierarquia
    children: List['ParsedNode'] = []

    # Metadados adicionais
    section_number: Optional[str] = None  # Ex: "153", "154a"
    is_footnote: bool = False
    footnote_id: Optional[str] = None
    has_cross_reference: bool = False

    class Config:
        # Permite referência circular para children
        arbitrary_types_allowed = True


class TableStructure(BaseModel):
    """Estrutura de tabela com metadados"""
    rows: List[List[ParsedNode]]  # Linhas e células
    headers: Optional[List[ParsedNode]] = None  # Cabeçalhos se houver
    caption: Optional[str] = None
    attributes: Dict[str, str] = {}


class ParsedDocument(BaseModel):
    """Documento HTML completamente parseado"""
    title: str
    encoding: str = "utf-8"

    # Estrutura do documento
    nodes: List[ParsedNode]

    # Índices para navegação rápida
    sections: Dict[str, ParsedNode] = {}  # ID da seção -> nó
    footnotes: Dict[str, ParsedNode] = {}  # ID da nota -> nó

    # Estatísticas
    stats: Dict[str, Any] = {
        "total_nodes": 0,
        "text_segments": 0,
        "latin_segments": 0,
        "english_segments": 0,
        "gloss_segments": 0,
        "tables": 0,
        "lists": 0
    }

    # Metadados do arquivo original
    original_filename: Optional[str] = None
    css_file: Optional[str] = None


# Permite referência circular
ParsedNode.model_rebuild()
