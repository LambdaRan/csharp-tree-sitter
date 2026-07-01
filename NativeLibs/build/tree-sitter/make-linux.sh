#!/bin/sh

set -e

echo Making tree-sitter on LINUX

cd "$(dirname "$0")"

OUTDIR="../../out/$1"
SRCDIR="../../submodule/tree-sitter"

mkdir -p "$OUTDIR"

# Clean
if [ "$1" = "clean" ]; then
    rm -f "$OUTDIR/libtree-sitter.so"
    echo Clean done.
    exit 0
fi

# Build using upstream Makefile with amalgamated source
make -C "$SRCDIR" AMALGAMATED=1 libtree-sitter.so

cp "$SRCDIR/libtree-sitter.so" "$OUTDIR/"

echo
echo "Build succeeded: $OUTDIR/libtree-sitter.so"
