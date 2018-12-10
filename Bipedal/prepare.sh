#!/bin/sh
if [ ! -e build/ ]; then
    mkdir build
fi
cp -r ml-agents/UnitySDK/Assets/ML-Agents environment/Assets/
cp models/* build/
