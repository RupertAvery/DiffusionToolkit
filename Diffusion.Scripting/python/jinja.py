from jinja2 import Template

template_str = """
score_9, score_8_up, score_7_up, score_6_up, brunette, medium breasts, {looking at viewer|}, 
{smiling|},
brunette, 
thighs,
nurse, stockings,  {lace bra|nude|uniform, white lab dress {lace bra|} {stethoscope|}},
{low angle,||}
{downblouse|{from behind, rear view|side view|}},
{plain background|dark background|},
"""

template = Template(template_str)


rendered = template.render()
print(rendered)