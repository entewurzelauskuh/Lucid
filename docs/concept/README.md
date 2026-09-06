# Concept art

Ten images pinning down the look and the feel of Lucid while it is being
written. They are on the front page of the repository, and the README says
what each one shows and which milestone it belongs to.

**They are not screenshots.** Nothing here was captured from a running build.
Most of what they show does not exist yet: there is no god view, no HUD, no
mob and no weapon in the project today. Read them as a target, not a report.

## How they were made

Generated with **Google Gemini** from prompts written against `docs/SPEC.md`
and `docs/UI.md`, then chosen and named by the owner. Nothing here was drawn
by hand or traced from another game. Two consequences worth knowing:

- **Text inside the images is not real.** The palette labels in the god view
  read "Sttret", "Essees", "Gleep" — the generator's invention, not cube names.
  Where an image shows convincing UI copy, `docs/UI.md` §14 is the actual
  glossary.
- **Geometry inside them is not to spec.** They are mood, not measurements;
  `docs/CUBE-SPEC.md` fixes the numbers.

## Licence

CC-BY-4.0, as the project's own work (`docs/SPEC.md` §18,
`docs/DECISIONS.md` 2026-09-06).

## Files

Committed at 1600 px wide — 820 CSS px of README column at 2× device pixel
ratio — encoded from the masters with ImageMagick at JPEG quality 80,
progressive, metadata stripped. About 1.08 MB the set. The two UI mock-ups
keep full 4:4:4 chroma because they carry small text; the rest are 4:2:0.

```
magick <master> -resize 1600x -strip -quality 80 \
  -sampling-factor 4:2:0 -interlace JPEG <name>.jpg
```

The first version of these was written with `sips -s formatOptions 80`, whose
scale is not JPEG quality: the files came out at quality 94 and 1.8 MB, and the
line above claimed 80. Review caught the artefacts disagreeing with the note.

The full-resolution masters (2752 × 1536, ~24 MB) are **not** committed — they
would sit in git history for ever to serve a page that wants web-sized copies.
Treat what is here as **write-once**: it is a plain blob, not LFS, so every
re-export stays in every clone's history for good.
`docs/concept/*.jpg` is also exempt from Git LFS in `.gitattributes`: GitHub
serves README images out of the LFS bandwidth quota, and ten of them on the
front page would spend it on people merely reading about the game.
