"""Utility to generate the MR Shop Privacy & Policy page.

Run this script whenever the policy content changes. It overwrites
`privacy.html` in the project root with the latest markup so the site
can link to a single, always-current HTML document.
"""

from __future__ import annotations

from datetime import datetime
from pathlib import Path
from textwrap import dedent

OUTPUT_PATH = Path(__file__).with_name("privacy.html")


def build_privacy_policy_html() -> str:
    """Return the full Privacy & Policy HTML document as a string."""
    last_updated = datetime.now().strftime("%B %d, %Y")

    sections = [
        (
            "1. Data We Collect",
            "We collect account details, purchase history, payment confirmation "
            "metadata, communications with customer support, and limited device "
            "information that helps us secure your sessions.",
        ),
        (
            "2. How We Use Your Information",
            "Your data lets us process orders, provide donation receipts, detect "
            "fraud, personalize recommendations, and comply with legal or tax "
            "requirements in the markets where we operate.",
        ),
        (
            "3. Security & Retention",
            "We apply encryption in transit and at rest, strict access controls, "
            "and routine audits. Data is retained only as long as necessary to "
            "deliver services or satisfy legal obligations.",
        ),
        (
            "4. Cookies & Similar Technologies",
            "Essential cookies keep you signed in, remember cart contents, and "
            "measure site performance. Optional analytics cookies help us "
            "improve features; you can disable them in your browser settings.",
        ),
        (
            "5. Your Rights",
            "Depending on your jurisdiction, you may request access, correction, "
            "deletion, portability, or restriction of processing. Submit requests "
            "through your account dashboard or by emailing our privacy team.",
        ),
        (
            "6. Contact Us",
            "For privacy questions, email privacy@mrshop.com or write to: "
            "MR Shop Privacy Office, House 18, Road 3, Dhaka, Bangladesh.",
        ),
    ]

    sections_html = "".join(
        f"""
        <section>
          <h2>{title}</h2>
          <p>{body}</p>
        </section>
        """
        for title, body in sections
    )

    html = f"""<!DOCTYPE html>
<html lang=\"en\">
<head>
  <meta charset=\"UTF-8\" />
  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />
  <title>Privacy & Policy — MR Shop</title>
  <style>
    :root {{
      --accent: #7c3aed;
      --accent-2: #06b6d4;
      --text: #0f172a;
      --muted: #4b5563;
      --border: #e2e8f0;
      --bg: #ffffff;
      --bg-alt: #f8fafc;
    }}

    * {{
      box-sizing: border-box;
      margin: 0;
      padding: 0;
    }}

    body {{
      font-family: 'Poppins', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
      background: var(--bg-alt);
      color: var(--text);
      line-height: 1.7;
      padding: 40px 16px 80px;
    }}

    .policy {{
      max-width: 900px;
      margin: 0 auto;
      background: var(--bg);
      border-radius: 18px;
      padding: 48px;
      box-shadow: 0 20px 70px rgba(15, 23, 42, 0.12);
      border: 1px solid var(--border);
    }}

    header {{
      margin-bottom: 32px;
    }}

    header h1 {{
      font-size: 2.5rem;
      background: linear-gradient(120deg, var(--accent), var(--accent-2));
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      margin-bottom: 12px;
    }}

    header p {{
      color: var(--muted);
      font-size: 0.95rem;
    }}

    section {{
      margin-top: 32px;
      padding-top: 32px;
      border-top: 1px solid var(--border);
    }}

    section:first-of-type {{
      border-top: none;
      padding-top: 0;
      margin-top: 0;
    }}

    section h2 {{
      font-size: 1.4rem;
      margin-bottom: 12px;
      color: var(--accent);
    }}

    section p {{
      font-size: 1rem;
      color: var(--text);
    }}

    @media (max-width: 640px) {{
      .policy {{
        padding: 32px 20px;
      }}

      header h1 {{
        font-size: 2rem;
      }}
    }}
  </style>
</head>
<body>
  <main class=\"policy\">
    <header>
      <h1>Privacy & Policy</h1>
      <p>Last updated: {last_updated}</p>
    </header>
    <p>
      We respect your privacy and explain below how MR Shop collects, uses, and protects
      your information when you shop, donate, or partner with us.
    </p>
    {sections_html}
  </main>
</body>
</html>
"""
    return dedent(html)


def write_privacy_policy(path: Path = OUTPUT_PATH) -> Path:
    """Write the privacy policy HTML to ``path`` and return the path."""
    html = build_privacy_policy_html()
    path.write_text(html, encoding="utf-8")
    return path


def main() -> None:
    path = write_privacy_policy()
    print(f"Privacy policy written to {path.resolve()}")


if __name__ == "__main__":
    main()
