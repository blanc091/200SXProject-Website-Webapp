#!/bin/bash

# Debugging output
echo "Running mask-content.sh on file: $1"

# Check if the script received an argument (file path)
if [ $# -eq 0 ]; then
    echo "No file specified."
    exit 1
fi

# Get the file path from the first argument
FILE="$1"

# Ensure the file exists before trying to modify it
if [ ! -f "$FILE" ]; then
    echo "File not found: $FILE"
    exit 1
fi

# Debugging: Confirm we’re about to overwrite the file
echo "Overwriting file with placeholder content"

# Replace content with a placeholder
echo "// File contents hidden" > "$FILE"

# Debugging: Verify the file contents
echo "Updated file contents:"
cat "$FILE"
