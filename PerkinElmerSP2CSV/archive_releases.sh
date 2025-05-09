#!/bin/zsh

#Expects version string as $1

function compress()
{
	#Expects input dir as $1 and output name as $2
	zip -r "bin/Release/$2.zip" "$1"
}

release_name="PE2CSV"
dir="bin/Release/netcoreapp3.1/linux-x64/publish"
if [[ -d $dir ]]; then
	compress $dir "$release_name-$1_lin-x64"
fi
dir="bin/Release/netcoreapp3.1/win-x64/publish"
if [[ -d $dir ]]; then
	compress $dir "$release_name-$1_win-x64"
fi
dir="bin/Release/netcoreapp3.1/win-x86/publish"
if [[ -d $dir ]]; then
	compress $dir "$release_name-$1_win-x86"
fi

