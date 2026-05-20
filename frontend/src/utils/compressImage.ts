const loadImage = (url: string): Promise<HTMLImageElement> =>
  new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => resolve(img);
    img.onerror = reject;
    img.src = url;
  });

const compressImage = async (
  file: File | null,
  maxSize: number = 400,
): Promise<File | undefined> => {
  if (!file) throw new Error("file failed");

  // Create new image
  const url = URL.createObjectURL(file);
  const img = await loadImage(url);

  const ratio = Math.min(maxSize / img.width, maxSize / img.height);

  const canvas = document.createElement("canvas");
  canvas.width = img.width * ratio;
  canvas.height = img.height * ratio;
  canvas.getContext("2d")?.drawImage(img, 0, 0, canvas.width, canvas.height);

  return new Promise((resolve, reject) => {
    canvas.toBlob(
      (blob) => {
        // Revoke object URL to free memory
        URL.revokeObjectURL(url);
        if (!blob) {
          reject(new Error("toBlob failed"));
          return;
        }
        resolve(
          new File([blob], file.name ?? "compressed.jpg", {
            type: "image/jpeg",
          }),
        );
      },
      "image/jpeg",
      0.8, // JPEG quality (0.0 - 1.0)
    );
  });
};

export default compressImage;
