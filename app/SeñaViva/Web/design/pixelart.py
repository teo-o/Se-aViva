"""Generates SeñaViva's pixel-art assets (logo, favicon and illustrations) from ASCII grids.

Each character is one pixel; "." is transparent. Colors match the tokens in wwwroot/app.css.
Run from this folder:  python pixelart.py
"""

from pathlib import Path

from PIL import Image

PALETTE = {
    "K": "#1c1b2b",  # ink
    "T": "#1f7a70",  # teal
    "D": "#145149",  # teal-deep
    "W": "#fffbf1",  # snow
    "P": "#e3d5b0",  # paper-deep
    "S": "#f2b83a",  # sun
    "C": "#c2452b",  # coral
}

# Speech bubble with a waving open hand (the sign) and motion marks: a sign that is alive.
LOGO = """
................
..KKKKKKKKKKKK..
.KTTTTTTTTTTTTK.
KTTTTTTWTWTTTSDK
KTTTTWTWTWTTSTDK
KTTTTWTWTWTTTTDK
KTTTTWTWTWTWTSDK
KTTWTWWWWWWWTSDK
KTTWWWWWWWWWTTDK
KTTTWWWWWWWWTTDK
KTTTTWWWWWWWTTDK
.KTTTTWWWWWDDDK.
..KTKKKKKKKKKK..
..KK............
................
................
"""

# Empty state: a conversation waiting to start.
EMPTY = """
................
..KKKKKKKKKKKK..
.KPPPPPPPPPPPPK.
KPPPPPPPPPPPPPPK
KPPPPPPPPPPPPPPK
KPPPPPPPPPPPPPPK
KPPPKKPPKKPPKKPK
KPPPKKPPKKPPKKPK
KPPPPPPPPPPPPPPK
KPPPPPPPPPPPPPPK
KPPPPPPPPPPPPPPK
.KPPPPPPPPPPPPK.
..KPKKKKKKKKKK..
..KK............
................
................
"""

# Not found / error: a question nobody can answer.
LOST = """
................
..KKKKKKKKKKKK..
.KWWWWWWWWWWWWK.
KWWWWWCCCWWWWWWK
KWWWWCWWWCWWWWWK
KWWWWWWWWCWWWWWK
KWWWWWWCCWWWWWWK
KWWWWWWCWWWWWWWK
KWWWWWWWWWWWWWWK
KWWWWWWCWWWWWWWK
KWWWWWWWWWWWWWWK
.KWWWWWWWWWWWWK.
..KWKKKKKKKKKK..
..KK............
................
................
"""


def rows(grid: str) -> list[str]:
    lines = [line for line in grid.strip("\n").splitlines()]
    width = len(lines[0])
    assert all(len(line) == width for line in lines), "all rows must have the same width"
    return lines


def to_svg(grid: str, title: str) -> str:
    lines = rows(grid)
    rects = []
    for y, line in enumerate(lines):
        x = 0
        while x < len(line):
            color = line[x]
            run = 1
            while x + run < len(line) and line[x + run] == color:
                run += 1
            if color != ".":
                rects.append(f'<rect x="{x}" y="{y}" width="{run}" height="1" fill="{PALETTE[color]}"/>')
            x += run
    size = f"{len(lines[0])} {len(lines)}"
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {size}" shape-rendering="crispEdges" role="img">'
        f"<title>{title}</title>{''.join(rects)}</svg>\n"
    )


def to_png(grid: str, scale: int, path: Path) -> None:
    lines = rows(grid)
    image = Image.new("RGBA", (len(lines[0]), len(lines)), (0, 0, 0, 0))
    for y, line in enumerate(lines):
        for x, color in enumerate(line):
            if color != ".":
                image.putpixel((x, y), Image.new("RGB", (1, 1), PALETTE[color]).getpixel((0, 0)) + (255,))
    image.resize((image.width * scale, image.height * scale), Image.NEAREST).save(path)


if __name__ == "__main__":
    out = Path(__file__).parent.parent / "wwwroot" / "img"
    out.mkdir(exist_ok=True)
    (out / "logo.svg").write_text(to_svg(LOGO, "SeñaViva"), encoding="utf-8")
    (out / "empty.svg").write_text(to_svg(EMPTY, "Sin datos"), encoding="utf-8")
    (out / "lost.svg").write_text(to_svg(LOST, "No encontrado"), encoding="utf-8")
    to_png(LOGO, 2, out.parent / "favicon.png")
    to_png(LOGO, 12, out / "logo-192.png")  # for Auth0 Universal Login, which needs a PNG URL
    print("assets generated in", out)
