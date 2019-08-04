sudo bluetoothctl <<EOF
power on
discoverable on
pairable on
agent NoInputNoOutput
default-agent
EOF
CUR=$(dirname $0);
sudo sdptool add --channel=3 SP
sudo rfcomm watch /dev/rfcomm0 3 mono "$CUR/bin/Debug/HexBot.exe"
