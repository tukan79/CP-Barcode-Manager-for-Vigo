# Technical Notes

## Barcode Rule

```text
filename without extension = 0080001234202606011125
skip first 2 characters     = 00
read next 8 digits          = 80001234
ignore the rest             = 202606011125
```

## PDF Placement

- Page: first page only.
- Position: bottom center.
- Bottom margin: 20 mm.
- Barcode size: 70 mm x 18 mm.

## Libraries

- PDFsharp: PDF editing/stamping.
- ZXing.Net + ZXing.Windows.Compatibility: Code 128 barcode generation.
