import math
import os
import struct
import wave

RATE = 44100
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Integrante1", "Audio")


def write(name, notes):
    """notes: lista de (frecuencia_inicial, frecuencia_final, segundos)"""
    samples = []
    phase = 0.0
    for f0, f1, dur in notes:
        n = int(RATE * dur)
        for i in range(n):
            t = i / n
            f = f0 + (f1 - f0) * t
            phase += 2 * math.pi * f / RATE
            env = min(1.0, i / 200) * (1 - t) ** 0.5
            samples.append(int(32767 * 0.4 * env * math.sin(phase)))
    path = os.path.join(OUT, name + ".wav")
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(b"".join(struct.pack("<h", s) for s in samples))
    print("escrito", os.path.normpath(path))


os.makedirs(OUT, exist_ok=True)
write("recarga", [(600, 1200, 0.2)])
write("impulso", [(150, 300, 0.15)])
write("dano", [(300, 80, 0.3)])
write("victoria", [(523, 523, 0.12), (659, 659, 0.12), (784, 784, 0.25)])
