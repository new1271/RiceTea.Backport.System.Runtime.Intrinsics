#!/bin/bash

if [ "$#" -lt 2 ]; then
    echo "Usage: $0 <static library path> <function name>"
    echo "Example: $0 libSignalHook.a signalHandler"
    exit 1
fi

LIB_A=$1
FUNC_NAME=$2
TEMP_O="temp_member.o"
BIN_OUT="extracted_code.bin"

MEMBER_O=$(nm $LIB_A | awk -v func_name=" T $FUNC_NAME" '
    /\.o:/ { last_o = $1 }
    $0 ~ func_name { print last_o; exit }
' | sed 's/://')

if [ -z "$MEMBER_O" ]; then
    echo "Error: Cannot found the function $FUNC_NAME in $LIB_A"
    exit 1
fi

echo "[1/4] Detected the object part: $MEMBER_O"

ar x $LIB_A $MEMBER_O
mv $MEMBER_O $TEMP_O

objcopy -O binary --only-section=.text $TEMP_O $BIN_OUT
echo "[2/4] Extracted the machine code into: $BIN_OUT"

INFO=$(nm -S $TEMP_O | grep " T $FUNC_NAME")
OFFSET_HEX=$(echo $INFO | awk '{print $1}')
SIZE_HEX=$(echo $INFO | awk '{print $2}')
SIZE_DEC=$((16#$SIZE_HEX))

echo "[3/4] Function information: Offset=0x$OFFSET_HEX, Size=$SIZE_DEC bytes"
echo "------------------------------------------------------------"
echo "[4/4] Layout:"
echo "------------------------------------------------------------"

START_ADDR=$((16#$OFFSET_HEX))
STOP_ADDR=$(($START_ADDR + $SIZE_DEC))

objdump -d -M intel --start-address=$START_ADDR --stop-address=$STOP_ADDR $TEMP_O

rm $TEMP_O