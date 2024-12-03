# File Picker API

## Overview

The File Picker API is designed to simplify the use of images on HTTP or HTTPS websites by providing a service that converts images to the WebP format and resizes them to the optimal dimensions for various uses. This API helps improve performance and streamline image management in web applications.

## Features

- **Layered Architecture**: The project is built using a clean, layered architecture for better organization and maintainability.
- **RESTful API for Image Management**: Provides endpoints for deleting, uploading, and fetching images.
- **Public Image Access**: Allows public access to images without requiring authorization.
- **Automatic Image Conversion to WebP**: Converts images to WebP format automatically when accessed, optimizing web performance.
- **Automatic Image Resizing**: Resizes images on-the-fly based on URL parameters, preserving the aspect ratio if requested.
- **Disk Caching**: Caches images on disk to avoid redundant resizing operations, improving efficiency.

## Endpoints

### Image Manager (Requires Authorization)

- **GET /api/ImageManager**: Retrieve a paginated list of available images. Requires `skip` and `take` parameters.
- **GET /api/ImageManager/GetTotalCount**: Retrieve the total number of available images.
- **GET /api/ImageManager/GetInfo/{id}**: Get detailed information about a specific image by its ID.
- **POST /api/ImageManager**: Upload a new image.
- **DELETE /api/ImageManager/{id}**: Delete an image by its ID.

### Public Image

- **GET /api/Image/{id}**: Retrieve an image by its ID. Supports optional parameters for width, height, and aspect ratio preservation. Only available in WebP format.
- **GET /api/Image/{id}.{extension}**: Retrieve an image by its ID in the specified format. Supports optional parameters for width, height, and aspect ratio preservation. Requires authorization.

## Usage

### Fetching an Image

To fetch an image, send a GET request to the `/api/Image/{id}` endpoint. You can include optional query parameters to specify the desired width, height, and whether to preserve the aspect ratio:

`GET /api/Image/{id}?width=200&height=200&preserveAspect=true`

If only the `width` or `height` parameter is provided, the other dimension will be adjusted automatically to preserve the original aspect ratio.

### Uploading an Image

To upload an image, send a POST request to the `/api/ImageManager` endpoint with the image file included in the request body:

`POST /api/ImageManager
Content-Type: multipart/form-data
Body: [image file]`

### Deleting an Image

To delete an image, send a DELETE request to the `/api/ImageManager/{id}` endpoint:

`DELETE /api/ImageManager/{id}`

## Image Caching

The API caches images on disk to improve performance. When an image is requested, the API checks if a cached version exists. If not, it resizes and converts the image as needed, then caches the result for future requests.

## Development and Contribution

### Requirements

- .NET 8

### Setup

1. Clone the repository:
git clone https://github.com/gabrielsimoest/FilePickerAPI.git

2. Navigate to the project directory:
cd FilePickerAPI

3. Restore dependencies:
dotnet restore

4. Run the application:
dotnet run

### Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes. Ensure your code follows the project's coding standards.

## License

This project is licensed under the GPL-3.0 License. See the LICENSE file for details.
