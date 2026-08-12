import type {
  Metadata,
} from "next";

import {
  Instrument_Sans,
  Newsreader,
} from "next/font/google";

import "./globals.css";

import {
  Providers,
} from "./providers";

const instrumentSans =
  Instrument_Sans({
    subsets: ["latin"],
    variable:
      "--font-instrument-sans",
    display: "swap",
  });

const newsreader =
  Newsreader({
    subsets: ["latin"],
    variable: "--font-newsreader",
    display: "swap",
  });

export const metadata: Metadata = {
  title: {
    default: "SlateDesk",
    template: "%s · SlateDesk",
  },
  description:
    "Academic work, clearly organized.",
};

const themeScript = `
(function () {
  try {
    var stored =
      localStorage.getItem("slatedesk-theme") ||
      "light";

    var resolved = stored;

    if (stored === "system") {
      resolved =
        window.matchMedia(
          "(prefers-color-scheme: dark)"
        ).matches
          ? "dark"
          : "light";
    }

    document.documentElement.dataset.theme =
      resolved;

    document.documentElement.dataset.themeMode =
      stored;
  } catch (_) {
    document.documentElement.dataset.theme =
      "light";

    document.documentElement.dataset.themeMode =
      "light";
  }
})();
`;

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="en"
      suppressHydrationWarning
    >
      <head>
        <script
          dangerouslySetInnerHTML={{
            __html: themeScript,
          }}
        />
      </head>

      <body
        className={`${instrumentSans.variable} ${newsreader.variable}`}
      >
        <a
          className="skip-link"
          href="#main-content"
        >
          Skip to main content
        </a>

        <Providers>
          {children}
        </Providers>
      </body>
    </html>
  );
}