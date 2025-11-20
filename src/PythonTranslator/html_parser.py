"""
Parser HTML robusto para documentos da gramática latina Allen & Greenough
"""
from bs4 import BeautifulSoup, Tag, NavigableString
from typing import List, Optional, Dict, Tuple
import re
from models import (
    ParsedDocument, ParsedNode, TextSegment, FormattingStyle,
    NodeType, TextType, TableStructure
)


class LatinGrammarParser:
    """Parser especializado para arquivos HTML da gramática latina"""

    # Classes CSS que identificam tipos de conteúdo
    LATIN_CLASSES = {'foreign'}
    GLOSS_CLASSES = {'gloss'}
    NOTE_CLASSES = {'noteLabel'}
    REFERENCE_CLASSES = {'bibl'}

    # Tags que devem ser processadas como blocos
    BLOCK_TAGS = {'p', 'h1', 'h2', 'h3', 'h4', 'div', 'blockquote', 'li'}

    # Tags que preservam formatação inline
    INLINE_TAGS = {'span', 'strong', 'em', 'b', 'i', 'u', 'a', 'sup', 'sub'}

    def __init__(self):
        self.stats = {
            "total_nodes": 0,
            "text_segments": 0,
            "latin_segments": 0,
            "english_segments": 0,
            "gloss_segments": 0,
            "tables": 0,
            "lists": 0
        }

    def parse_html(self, html_content: str, filename: Optional[str] = None) -> ParsedDocument:
        """
        Parseia conteúdo HTML e retorna documento estruturado

        Args:
            html_content: String com conteúdo HTML
            filename: Nome do arquivo original (opcional)

        Returns:
            ParsedDocument com estrutura completa
        """
        soup = BeautifulSoup(html_content, 'lxml')

        # Extrair metadados
        title = self._extract_title(soup)
        encoding = self._extract_encoding(soup)
        css_file = self._extract_css_link(soup)

        # Resetar estatísticas
        self.stats = {
            "total_nodes": 0,
            "text_segments": 0,
            "latin_segments": 0,
            "english_segments": 0,
            "gloss_segments": 0,
            "tables": 0,
            "lists": 0
        }

        # Parsear corpo do documento
        body = soup.find('body')
        if not body:
            body = soup  # Fallback se não houver tag body

        # Processar nós principais
        nodes = []
        sections = {}
        footnotes = {}

        for child in body.children:
            if isinstance(child, Tag):
                parsed = self._parse_element(child)
                if parsed:
                    nodes.append(parsed)

                    # Indexar por ID se existir
                    if parsed.node_id:
                        sections[parsed.node_id] = parsed

                    # Indexar footnotes
                    if parsed.is_footnote and parsed.footnote_id:
                        footnotes[parsed.footnote_id] = parsed

        return ParsedDocument(
            title=title,
            encoding=encoding,
            nodes=nodes,
            sections=sections,
            footnotes=footnotes,
            stats=self.stats.copy(),
            original_filename=filename,
            css_file=css_file
        )

    def _parse_element(self, element: Tag) -> Optional[ParsedNode]:
        """
        Parseia um elemento HTML recursivamente

        Args:
            element: Tag BeautifulSoup

        Returns:
            ParsedNode ou None se elemento deve ser ignorado
        """
        tag_name = element.name.lower()

        # Ignorar tags de script, style, etc.
        if tag_name in {'script', 'style', 'meta', 'link', 'head'}:
            return None

        # Mapear tag para NodeType
        node_type = self._get_node_type(tag_name)
        if not node_type:
            return None

        # Extrair atributos (converter listas em strings)
        attributes = {}
        for key, value in element.attrs.items():
            if isinstance(value, list):
                # Atributos como 'class' vêm como lista no BeautifulSoup
                attributes[key] = ' '.join(value)
            else:
                attributes[key] = value

        node_id = attributes.get('id')
        inline_style = attributes.get('style')

        # Extrair número de seção se existir
        section_number = self._extract_section_number(element)

        # Verificar se é footnote
        is_footnote, footnote_id = self._check_footnote(element)

        # Criar nó
        node = ParsedNode(
            node_type=node_type,
            node_id=node_id,
            attributes=attributes,
            inline_style=inline_style,
            section_number=section_number,
            is_footnote=is_footnote,
            footnote_id=footnote_id
        )

        # Processar conteúdo do elemento
        if tag_name == 'table':
            # Processar tabela especialmente
            self._parse_table(element, node)
            self.stats["tables"] += 1
        elif tag_name in {'ol', 'ul'}:
            # Processar lista
            self._parse_list(element, node)
            self.stats["lists"] += 1
        else:
            # Processar conteúdo misto (texto + elementos filhos)
            self._parse_content(element, node)

        self.stats["total_nodes"] += 1
        return node

    def _parse_content(self, element: Tag, node: ParsedNode):
        """
        Parseia conteúdo misto de um elemento (texto + tags inline + tags block)

        Args:
            element: Tag BeautifulSoup
            node: ParsedNode para preencher
        """
        for child in element.children:
            if isinstance(child, NavigableString):
                # Texto direto
                text = str(child).strip()
                if text:
                    segment = self._create_text_segment(
                        text=text,
                        text_type=TextType.ENGLISH,  # Assume inglês por padrão
                        element=element
                    )
                    node.text_segments.append(segment)
                    self.stats["text_segments"] += 1
                    self.stats["english_segments"] += 1

            elif isinstance(child, Tag):
                tag_name = child.name.lower()

                if tag_name in self.INLINE_TAGS:
                    # Processar tag inline (span, strong, em, etc.)
                    self._parse_inline_element(child, node)
                elif tag_name in self.BLOCK_TAGS:
                    # Elemento filho de bloco (recursivo)
                    child_node = self._parse_element(child)
                    if child_node:
                        node.children.append(child_node)

    def _parse_inline_element(self, element: Tag, parent_node: ParsedNode):
        """
        Parseia elemento inline preservando formatação

        Args:
            element: Tag inline (span, strong, em, a, etc.)
            parent_node: Nó pai onde adicionar segmentos
        """
        # Determinar tipo de texto pela classe CSS
        text_type = self._determine_text_type(element)

        # Extrair formatação
        formatting = self._extract_formatting(element)

        # Processar conteúdo do elemento inline
        text_content = []
        for child in element.children:
            if isinstance(child, NavigableString):
                text_content.append(str(child))
            elif isinstance(child, Tag):
                # Inline dentro de inline (ex: <span><strong>text</strong></span>)
                text_content.append(child.get_text())

        text = ''.join(text_content).strip()
        if text:
            segment = TextSegment(
                text=text,
                text_type=text_type,
                formatting=formatting,
                html_class=element.get('class', [None])[0] if element.get('class') else None
            )
            parent_node.text_segments.append(segment)

            # Atualizar estatísticas
            self.stats["text_segments"] += 1
            if text_type == TextType.LATIN:
                self.stats["latin_segments"] += 1
            elif text_type == TextType.ENGLISH:
                self.stats["english_segments"] += 1
            elif text_type == TextType.GLOSS:
                self.stats["gloss_segments"] += 1

    def _parse_table(self, table: Tag, node: ParsedNode):
        """
        Parseia estrutura de tabela

        Args:
            table: Tag <table>
            node: ParsedNode da tabela
        """
        for row in table.find_all('tr', recursive=False):
            row_node = ParsedNode(
                node_type=NodeType.TABLE_ROW,
                attributes=dict(row.attrs)
            )

            for cell in row.find_all(['td', 'th'], recursive=False):
                cell_type = NodeType.TABLE_HEADER if cell.name == 'th' else NodeType.TABLE_CELL
                cell_node = ParsedNode(
                    node_type=cell_type,
                    attributes=dict(cell.attrs),
                    inline_style=cell.get('style')
                )

                # Parsear conteúdo da célula
                self._parse_content(cell, cell_node)
                row_node.children.append(cell_node)

            node.children.append(row_node)

    def _parse_list(self, list_element: Tag, node: ParsedNode):
        """
        Parseia lista (ol/ul)

        Args:
            list_element: Tag <ol> ou <ul>
            node: ParsedNode da lista
        """
        for item in list_element.find_all('li', recursive=False):
            item_node = self._parse_element(item)
            if item_node:
                node.children.append(item_node)

    def _determine_text_type(self, element: Tag) -> TextType:
        """
        Determina o tipo de texto baseado nas classes CSS

        Args:
            element: Tag HTML

        Returns:
            TextType apropriado
        """
        classes = element.get('class', [])

        # Verificar classes conhecidas
        if any(cls in self.LATIN_CLASSES for cls in classes):
            return TextType.LATIN
        elif any(cls in self.GLOSS_CLASSES for cls in classes):
            return TextType.GLOSS
        elif any(cls in self.REFERENCE_CLASSES for cls in classes):
            return TextType.REFERENCE

        # Padrão: inglês
        return TextType.ENGLISH

    def _extract_formatting(self, element: Tag) -> FormattingStyle:
        """
        Extrai estilo de formatação do elemento

        Args:
            element: Tag HTML

        Returns:
            FormattingStyle com propriedades extraídas
        """
        formatting = FormattingStyle()

        # Formatação por tag
        tag_name = element.name.lower()
        if tag_name in {'strong', 'b'}:
            formatting.bold = True
        elif tag_name in {'em', 'i'}:
            formatting.italic = True
        elif tag_name == 'u':
            formatting.underline = True

        # Formatação por estilo inline
        style = element.get('style', '')
        if 'font-weight: bold' in style or 'font-weight:bold' in style:
            formatting.bold = True
        if 'font-style: italic' in style or 'font-style:italic' in style:
            formatting.italic = True
        if 'text-decoration: underline' in style:
            formatting.underline = True

        # Extrair padding-left
        padding_match = re.search(r'padding-left:\s*(\d+px)', style)
        if padding_match:
            formatting.padding_left = padding_match.group(1)

        # Extrair text-align
        align_match = re.search(r'text-align:\s*(\w+)', style)
        if align_match:
            formatting.text_align = align_match.group(1)

        return formatting

    def _create_text_segment(
        self,
        text: str,
        text_type: TextType,
        element: Tag
    ) -> TextSegment:
        """
        Cria um TextSegment com formatação do elemento pai

        Args:
            text: Texto do segmento
            text_type: Tipo de texto
            element: Elemento HTML pai

        Returns:
            TextSegment configurado
        """
        formatting = self._extract_formatting(element)

        return TextSegment(
            text=text,
            text_type=text_type,
            formatting=formatting
        )

    def _extract_section_number(self, element: Tag) -> Optional[str]:
        """
        Extrai número de seção se presente (ex: "153", "154a")

        Args:
            element: Tag HTML

        Returns:
            Número da seção ou None
        """
        # Procurar por <strong>número</strong> no início do parágrafo
        strong = element.find('strong')
        if strong:
            text = strong.get_text().strip()
            # Padrão: número opcionalmente seguido de letra
            match = re.match(r'^(\d+[a-z]?)\.?$', text)
            if match:
                return match.group(1)

        return None

    def _check_footnote(self, element: Tag) -> Tuple[bool, Optional[str]]:
        """
        Verifica se elemento é uma footnote

        Args:
            element: Tag HTML

        Returns:
            (is_footnote, footnote_id)
        """
        # Procurar por links de footnote
        link = element.find('a')
        if link and link.get('id'):
            link_id = link.get('id')
            # Padrão: fn1, fn2, rfn1, rfn2, etc.
            if link_id.startswith('fn') or link_id.startswith('rfn'):
                return True, link_id

        return False, None

    def _get_node_type(self, tag_name: str) -> Optional[NodeType]:
        """
        Mapeia tag HTML para NodeType

        Args:
            tag_name: Nome da tag

        Returns:
            NodeType correspondente ou None
        """
        mapping = {
            'h1': NodeType.HEADING_1,
            'h2': NodeType.HEADING_2,
            'h3': NodeType.HEADING_3,
            'h4': NodeType.HEADING_4,
            'p': NodeType.PARAGRAPH,
            'ol': NodeType.LIST_ORDERED,
            'ul': NodeType.LIST_UNORDERED,
            'li': NodeType.LIST_ITEM,
            'table': NodeType.TABLE,
            'tr': NodeType.TABLE_ROW,
            'td': NodeType.TABLE_CELL,
            'th': NodeType.TABLE_HEADER,
            'blockquote': NodeType.BLOCKQUOTE,
            'div': NodeType.DIV,
            'span': NodeType.SPAN,
            'a': NodeType.LINK,
            'strong': NodeType.STRONG,
            'em': NodeType.EMPHASIS,
        }
        return mapping.get(tag_name)

    def _extract_title(self, soup: BeautifulSoup) -> str:
        """Extrai título do documento"""
        title_tag = soup.find('title')
        return title_tag.get_text().strip() if title_tag else "Untitled"

    def _extract_encoding(self, soup: BeautifulSoup) -> str:
        """Extrai encoding do documento"""
        meta = soup.find('meta', attrs={'http-equiv': 'Content-Type'})
        if meta and 'content' in meta.attrs:
            content = meta['content']
            match = re.search(r'charset=([\w-]+)', content)
            if match:
                return match.group(1)
        return "utf-8"

    def _extract_css_link(self, soup: BeautifulSoup) -> Optional[str]:
        """Extrai link para arquivo CSS"""
        link = soup.find('link', attrs={'rel': 'stylesheet'})
        return link.get('href') if link else None
