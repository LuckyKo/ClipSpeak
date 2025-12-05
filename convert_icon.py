from PIL import Image
import sys

def convert_to_ico(input_path, output_path):
    try:
        img = Image.open(input_path)
        # Create multiple sizes for the ICO
        icon_sizes = [(256, 256), (128, 128), (64, 64), (48, 48), (32, 32), (16, 16)]
        img.save(output_path, format='ICO', sizes=icon_sizes)
        print(f"Successfully converted {input_path} to {output_path}")
    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    convert_to_ico("ClipSpeakIcon.png", "app.ico")
