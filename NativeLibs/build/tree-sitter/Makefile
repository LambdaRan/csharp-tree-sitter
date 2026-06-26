CFLAGS=/nologo /FC /std:c11 /utf-8 /Gy /diagnostics:column /I../../submodule/tree-sitter/lib/include
SRCDIR=../../submodule/tree-sitter/lib/src
OUTDIR=..\..\out

all: $(OUTDIR)\tree-sitter.dll

$(OUTDIR)\tree-sitter.obj: \
		$(SRCDIR)/alloc.c \
		$(SRCDIR)/get_changed_ranges.c \
		$(SRCDIR)/language.c \
		$(SRCDIR)/lexer.c \
		$(SRCDIR)/lib.c \
		$(SRCDIR)/node.c \
		$(SRCDIR)/parser.c \
		$(SRCDIR)/query.c \
		$(SRCDIR)/stack.c \
		$(SRCDIR)/subtree.c \
		$(SRCDIR)/tree.c \
		$(SRCDIR)/tree_cursor.c
	cl $(CFLAGS) /Fo:$@ /I$(SRCDIR) /I$(SRCDIR)/unicode /c $(SRCDIR)/lib.c

$(OUTDIR)\tree-sitter.dll: $(OUTDIR)\tree-sitter.obj
	cl /LD $(CFLAGS) /Fe:$@ $(OUTDIR)\tree-sitter.obj /link /def:tree-sitter.def /incremental:no

clean:
	-del $(OUTDIR)\tree-sitter.obj 2>nul
	-del $(OUTDIR)\tree-sitter.dll $(OUTDIR)\tree-sitter.exp $(OUTDIR)\tree-sitter.lib $(OUTDIR)\tree-sitter.pdb 2>nul

# End of File.
