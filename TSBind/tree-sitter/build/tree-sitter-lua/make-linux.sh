#!/bin/sh

set -e

echo Making tree-sitter-lua on LINUX

cd "$(dirname "$0")"

OUTDIR="../../out"
SRCDIR="../../submodule/tree-sitter-lua"

mkdir -p "$OUTDIR"

# Clean
if [ "$1" = "clean" ]; then
    rm -f "$OUTDIR/libtree-sitter-lua.so"
    echo Clean done.
    exit 0
fi

# Build using upstream Makefile
make -C "$SRCDIR"

cp "$SRCDIR/libtree-sitter-lua.so" "$OUTDIR/"

echo
echo "Build succeeded: $OUTDIR/libtree-sitter-lua.so"
