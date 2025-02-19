import os

def rename_files():
	folder_path = os.path.dirname(os.path.abspath(__file__))  # Get the script's directory
	for filename in os.listdir(folder_path):
		print(f'Checking: {filename}')
		if "_" in filename:
			print(f'Has score: {filename}')
			parts = filename.rsplit("_", 1)  # Split on the last underscore
			new_name = f"ThinkerShellGore{parts[1]}"
			old_path = os.path.join(folder_path, filename)
			new_path = os.path.join(folder_path, new_name)
			os.rename(old_path, new_path)
			print(f'Renamed: {filename} -> {new_name}')

# Execute the function
rename_files()
