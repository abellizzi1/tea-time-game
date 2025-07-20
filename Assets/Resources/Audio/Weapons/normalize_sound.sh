#!/usr/bin/bash

mkdir normalized

for f in *.wav; do
  ffmpeg -i "$f" -af loudnorm=I=-16:TP=-1.5:LRA=11 "normalized/$f"
done
