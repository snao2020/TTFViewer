TTFViewer
    Show .ttf/.ttc file

* TTFViewer is based on [The OpenType Font File](https://docs.microsoft.com/en-us/typography/opentype/spec/otff),
and I changed what I could not understand.

1. TTCHeader Version 1.0
  TAG ttcTag --> Tag ttcTag (typo ??)

2. TTCHeader Version 2.0
  TAG ttcTag --> Tag ttcTag (typo ??)
  majorVersion = 2 --> = 1 or 2 (some ttf file of version1 have dsig)
  uint32 dsigTag --> Tag dsigTag
      (dsigTag may be zero, so uint32 is correct, but Tag is easier to handle)
  uint32 dsigOffset --> Offset32 dsigTag
      (dsigOffset may be zero, so uint32 is correct, but Offset32 is easier to handle)

3. 'cmap' Subtable Format 2
  I could not find the size of array SubHeader subHeaders[], so I set size=0.
  I could not find the size of array nuint16 glyphIndexArray[], so I set size=0.

4. 'cmap' Subtable Format 4
  I could not find the size of array uint16 glyphIdArray[], so I set size=0.

5. 'cmap' Subtable Format 10
  I could not find the size of array uint16 glyphs[], so I set size=0.

6. 'post' Header
  FWord underlinePosition --> FWORD underlinePosition (typo ??)
  FWord underlineThickness --> FWORD underlineThickness (typo ??)

7. 'post' Version2.0
  int8 names[numberNewGlyphs] --> PascalString names[numberNewGlyphs]

 

