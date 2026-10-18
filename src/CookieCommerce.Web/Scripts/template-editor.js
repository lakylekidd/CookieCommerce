import { basicSetup } from "codemirror";
import { EditorView, keymap } from "@codemirror/view";
import { EditorState } from "@codemirror/state";
import { html } from "@codemirror/lang-html";
import { defaultKeymap, indentWithTab } from "@codemirror/commands";

const editors = new Map();

const editorTheme = EditorView.theme({
    "&": {
        backgroundColor: "var(--template-editor-background, #ffffff)",
        color: "var(--template-editor-foreground, #111827)",
        fontSize: "0.875rem"
    },
    ".cm-content": {
        minHeight: "30rem",
        caretColor: "var(--template-editor-foreground, #111827)"
    },
    ".cm-gutters": {
        backgroundColor: "var(--template-editor-gutter, #f3f4f6)",
        color: "var(--template-editor-gutter-foreground, #6b7280)",
        borderRight: "1px solid var(--template-editor-border, #d1d5db)"
    },
    ".cm-activeLine, .cm-activeLineGutter": {
        backgroundColor: "var(--template-editor-active-line, #f9fafb)"
    },
    ".cm-scroller": {
        overflow: "auto",
        fontFamily: "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace"
    }
});

function disposeDetachedEditor(editorId, host) {
    const previous = editors.get(editorId);
    if (previous && previous.host !== host) {
        previous.view.destroy();
        editors.delete(editorId);
    }
}

function sync(editorId, textareaId, value, version) {
    const host = document.getElementById(editorId);
    const textarea = document.getElementById(textareaId);
    if (!host || !textarea) return;

    disposeDetachedEditor(editorId, host);
    let record = editors.get(editorId);
    if (!record) {
        const updateListener = EditorView.updateListener.of(update => {
            if (!update.docChanged || record?.syncing) return;
            textarea.value = update.state.doc.toString();
            textarea.dispatchEvent(new Event("input", { bubbles: true }));
        });

        const view = new EditorView({
            parent: host,
            state: EditorState.create({
                doc: value ?? "",
                extensions: [
                    basicSetup,
                    html(),
                    editorTheme,
                    keymap.of([...defaultKeymap, indentWithTab]),
                    updateListener
                ]
            })
        });
        record = { host, view, version, syncing: false };
        editors.set(editorId, record);
        textarea.value = value ?? "";
        return;
    }

    if (record.version !== version) {
        const nextValue = value ?? "";
        record.syncing = true;
        try {
            record.view.dispatch({
                changes: { from: 0, to: record.view.state.doc.length, insert: nextValue }
            });
            textarea.value = nextValue;
            record.version = version;
        } finally {
            record.syncing = false;
        }
    }
}

window.commerceTemplateEditor = { sync };
