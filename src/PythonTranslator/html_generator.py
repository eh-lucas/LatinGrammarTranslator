"""
Gerador de HTML a partir de ParsedDocument traduzido
Reconstrói HTML completo mantendo formatação e estrutura
"""
from models import ParsedDocument, ParsedNode, TextSegment, NodeType, TextType, FormattingStyle
from typing import Optional, List
import html


class HtmlGenerator:
    """Gera HTML a partir de documento parseado e traduzido"""

    def __init__(self, css_file: str = "./site.css"):
        self.css_file = css_file
        self.indent_level = 0
        self.indent_size = 2

    def generate_html(self, parsed_doc: ParsedDocument, output_path: str):
        """
        Gera arquivo HTML completo a partir de ParsedDocument

        Args:
            parsed_doc: Documento parseado e traduzido
            output_path: Caminho para salvar arquivo .html
        """
        print(f"\n{'='*80}")
        print(f"GERANDO HTML TRADUZIDO")
        print(f"{'='*80}\n")

        html_content = self._build_complete_html(parsed_doc)

        # Salvar arquivo
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(html_content)

        print(f"[OK] HTML salvo: {output_path}")
        print(f"  - Título: {parsed_doc.title}")
        print(f"  - Total de nodes: {parsed_doc.stats.get('total_nodes', 0)}")
        print(f"  - Encoding: {parsed_doc.encoding}\n")

    def _build_complete_html(self, parsed_doc: ParsedDocument) -> str:
        """Constrói documento HTML completo"""

        # Cabeçalho HTML
        html_parts = [
            '<!DOCTYPE html>',
            '<html>',
            '',
            '<head>',
            f'  <title>{html.escape(parsed_doc.title)}</title>',
            '  <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />',
            f'  <link rel="stylesheet" href="{self.css_file}" />',
            '</head>',
            '',
            '<body class="simple">',
            '  <div id="page-wrapper">',
        ]

        # Processar todos os nodes do documento
        self.indent_level = 2  # Começa dentro de page-wrapper
        for node in parsed_doc.nodes:
            html_parts.append(self._node_to_html(node))

        # Rodapé HTML
        html_parts.extend([
            '  </div>',
            '</body>',
            '',
            '</html>'
        ])

        return '\n'.join(html_parts)

    def _node_to_html(self, node: ParsedNode) -> str:
        """
        Converte um ParsedNode em HTML

        Args:
            node: Nó a converter

        Returns:
            String HTML do nó
        """
        indent = ' ' * (self.indent_level * self.indent_size)

        # Mapear NodeType para tag HTML
        tag = self._get_html_tag(node.node_type)

        # Construir atributos
        attributes = self._build_attributes(node)
        attr_str = ' ' + ' '.join(attributes) if attributes else ''

        # Caso especial para tags auto-fecháveis (se houver)
        if tag in ['br', 'hr', 'img']:
            return f"{indent}<{tag}{attr_str} />"

        # Construir conteúdo interno
        content_parts = []

        # Adicionar segmentos de texto
        if node.text_segments:
            content_parts.append(self._build_text_content(node.text_segments))

        # Processar children recursivamente
        if node.children:
            self.indent_level += 1
            for child in node.children:
                content_parts.append('\n' + self._node_to_html(child))
            self.indent_level -= 1

            # Se tem children, fechar tag em linha separada
            if node.children:
                return f"{indent}<{tag}{attr_str}>{''.join(content_parts)}\n{indent}</{tag}>"

        # Tag simples com conteúdo inline
        content = ''.join(content_parts)
        return f"{indent}<{tag}{attr_str}>{content}</{tag}>"

    def _get_html_tag(self, node_type: NodeType) -> str:
        """Mapeia NodeType para tag HTML"""
        # NodeType já contém o valor da tag (h1, p, etc)
        return node_type.value

    def _build_attributes(self, node: ParsedNode) -> List[str]:
        """Constrói lista de atributos HTML para o nó"""
        attrs = []

        # ID
        if node.node_id:
            attrs.append(f'id="{html.escape(node.node_id)}"')

        # Atributos preservados
        for key, value in node.attributes.items():
            escaped_value = html.escape(str(value))
            attrs.append(f'{key}="{escaped_value}"')

        # Style inline
        if node.inline_style:
            # Verificar se já não está nos attributes
            if 'style' not in node.attributes:
                attrs.append(f'style="{html.escape(node.inline_style)}"')

        return attrs

    def _build_text_content(self, segments: List[TextSegment]) -> str:
        """
        Constrói conteúdo textual a partir de segmentos

        Args:
            segments: Lista de segmentos de texto

        Returns:
            String HTML com texto formatado
        """
        html_parts = []

        for i, segment in enumerate(segments):
            text_html = self._segment_to_html(segment)
            html_parts.append(text_html)

            # Adicionar espaço entre segmentos, exceto:
            # - Após o último segmento
            # - Se o próximo segmento começa com pontuação
            if i < len(segments) - 1:
                next_segment = segments[i + 1]
                # Não adicionar espaço antes de pontuação ou se o texto atual termina com espaço
                if (not segment.text.endswith((' ', '\n', '\t')) and
                    not next_segment.text.startswith((' ', '\n', '\t', '.', ',', ':', ';', '!', '?', ')', ']', '}'))):
                    html_parts.append(' ')

        return ''.join(html_parts)

    def _segment_to_html(self, segment: TextSegment) -> str:
        """
        Converte um TextSegment em HTML

        Args:
            segment: Segmento de texto

        Returns:
            String HTML do segmento
        """
        text = html.escape(segment.text)

        # Construir wrapper com classe CSS se necessário
        needs_wrapper = False
        wrapper_attrs = []

        # Adicionar classe CSS baseada no tipo de texto
        if segment.html_class:
            wrapper_attrs.append(f'class="{segment.html_class}"')
            needs_wrapper = True
        elif segment.text_type == TextType.LATIN:
            # Latim sempre tem classe "foreign"
            wrapper_attrs.append('class="foreign"')
            needs_wrapper = True
        elif segment.text_type == TextType.GLOSS:
            # Gloss tem classe "gloss" se não tiver outra classe
            wrapper_attrs.append('class="gloss"')
            needs_wrapper = True

        # Adicionar style inline se necessário
        style_parts = self._build_inline_style(segment.formatting)
        if style_parts:
            wrapper_attrs.append(f'style="{"; ".join(style_parts)}"')
            needs_wrapper = True

        # Aplicar formatação com tags HTML apropriadas
        if segment.formatting.bold:
            text = f'<strong>{text}</strong>'
        if segment.formatting.italic:
            text = f'<em>{text}</em>'
        if segment.formatting.underline:
            text = f'<u>{text}</u>'

        # Envolver em span se necessário
        if needs_wrapper:
            attrs_str = ' '.join(wrapper_attrs)
            return f'<span {attrs_str}>{text}</span>'
        else:
            return text

    def _build_inline_style(self, formatting: FormattingStyle) -> List[str]:
        """
        Constrói lista de propriedades CSS inline

        Args:
            formatting: Objeto de formatação

        Returns:
            Lista de strings CSS (sem bold/italic/underline que viram tags)
        """
        style_parts = []

        if formatting.font_size:
            style_parts.append(f'font-size: {formatting.font_size}')
        if formatting.font_family:
            style_parts.append(f'font-family: {formatting.font_family}')
        if formatting.color:
            style_parts.append(f'color: {formatting.color}')
        if formatting.padding_left:
            style_parts.append(f'padding-left: {formatting.padding_left}')
        if formatting.text_align:
            style_parts.append(f'text-align: {formatting.text_align}')

        # Adicionar italic como style se não foi convertido para <em>
        # (já aplicamos como tag, então não precisamos aqui)

        return style_parts


def generate_translated_html(parsed_doc: ParsedDocument, output_path: str):
    """
    Função helper para gerar HTML traduzido

    Args:
        parsed_doc: Documento parseado e traduzido
        output_path: Caminho para salvar arquivo .html
    """
    generator = HtmlGenerator()
    generator.generate_html(parsed_doc, output_path)
