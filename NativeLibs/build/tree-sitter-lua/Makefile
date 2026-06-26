CFLAGS=/nologo /FC /std:c11 /utf-8 /Gy /diagnostics:column
SRCDIR=../../submodule/tree-sitter-lua/src
TS_INC=../../submodule/tree-sitter/lib/include
OUTDIR=..\..\out

all: $(OUTDIR)\tree-sitter-lua.dll

$(OUTDIR)\parser.obj: $(SRCDIR)/parser.c
	cl $(CFLAGS) /Fo:$@ /I$(SRCDIR) /I$(TS_INC) /c $(SRCDIR)/parser.c

$(OUTDIR)\scanner.obj: $(SRCDIR)/scanner.c
	cl $(CFLAGS) /Fo:$@ /I$(SRCDIR) /I$(TS_INC) /c $(SRCDIR)/scanner.c

$(OUTDIR)\tree-sitter-lua.dll: $(OUTDIR)\parser.obj $(OUTDIR)\scanner.obj
	cl /LD $(CFLAGS) /Fe:$@ $(OUTDIR)\parser.obj $(OUTDIR)\scanner.obj /link /incremental:no /export:tree_sitter_lua

clean:
	-del $(OUTDIR)\parser.obj $(OUTDIR)\scanner.obj 2>nul
	-del $(OUTDIR)\tree-sitter-lua.dll $(OUTDIR)\tree-sitter-lua.exp $(OUTDIR)\tree-sitter-lua.lib $(OUTDIR)\tree-sitter-lua.pdb 2>nul

# End of File.
