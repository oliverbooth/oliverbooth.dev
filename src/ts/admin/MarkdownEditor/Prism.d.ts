declare interface PrismLanguage {
}

declare interface Prism {
    languages: {
        markdown: PrismLanguage
    };

    highlight(markdown: string, language: PrismLanguage): string;
    highlightAllUnder(element: HTMLElement): void;
}

declare const Prism: Prism;
