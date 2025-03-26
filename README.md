# SwarmUI APIBackends Extension
===========================================================================

![APIBackends](url_to_image_placeholder)

## Table of Contents
-----------------

1. [Introduction](#introduction)
2. [Features](#features)
3. [Prerequisites](#prerequisites)
4. [Installation](#installation)
5. [Usage](#usage)
6. [Configuration](#configuration)
7. [Troubleshooting](#troubleshooting)
8. [Changelog](#changelog)
9. [License](#license)
10. [Contributing](#contributing)
11. [Acknowledgments](#acknowledgments)

## Introduction
---------------

The APIBackends Extension for SwarmUI enables integration with various commercial image generation APIs, including OpenAI's DALL-E, Anthropic's Claude, Black Forest Labs' Flux, and Ideogram. This extension allows you to use these services directly within the SwarmUI interface, providing a seamless experience for generating images using different AI models.

* Usage Example:
- Generate images using DALL-E 3
- Use Anthropic's Claude 3 for image generation
- Access Black Forest Labs' Flux models
- Generate with Ideogram's AI
- Seamlessly switch between different API providers

> [!WARNING]
> API usage incurs costs from the respective providers. Make sure you understand the pricing before use.
> Always keep your API keys secure and never share them.

## Features
------------

* Support for multiple API providers:
  - OpenAI (DALL-E 2 & 3)
  - Anthropic (Claude 3 Opus & Sonnet)
  - Black Forest Labs (Flux models)
  - Ideogram
* Integrated parameter controls for each provider
* Automatic model switching and parameter adjustment
* Secure API key management
* Custom base URL support for enterprise deployments
* Provider-specific parameter controls
* Permission system integration for access control

> [!NOTE]
> Future Features:
> - Additional API providers
> - Batch processing optimization
> - Cost estimation before generation
> - Usage tracking and reporting
> - Advanced parameter presets

## Prerequisites
----------------

Before installing the APIBackends Extension, ensure you have:
- SwarmUI installed and running
- Valid API keys for the services you plan to use
- Understanding of the associated costs and usage limits

## Installation
--------------

### Preferred Method (Via SwarmUI)

1. Open your SwarmUI instance
2. Navigate to the Server → Extensions tab
3. Find "APIBackends Extension" in the list
4. Click the Install button
5. Restart SwarmUI when prompted

### Manual Installation

If you prefer to install manually:

1. Close SwarmUI
2. Navigate to the `SwarmUI/src/Extensions` directory
3. Clone the APIBackends repository
4. Run `update-windows.bat` or `update-linuxmac.sh`
5. Restart SwarmUI and refresh your browser

## Usage
--------

1. Configure your API keys in the settings
2. Select your desired API provider model
3. Adjust provider-specific parameters
4. Generate images as normal using SwarmUI's interface
5. Images will be generated using the selected API service

## Configuration
----------------

Each API provider requires specific configuration:

* OpenAI:
  - API Key
  - Model selection (DALL-E 2 or 3)
  - Quality and style parameters

* Anthropic:
  - API Key
  - Model selection (Opus/Sonnet)
  - Quality settings

* Black Forest Labs:
  - API Key
  - Model selection
  - Custom endpoint (optional)

* Ideogram:
  - API Key
  - Style preferences
  - Resolution settings

## Troubleshooting
-----------------

Common issues and solutions:

* Check API key validity and permissions
* Verify network connectivity to API endpoints
* Confirm sufficient API credits/balance
* Check SwarmUI logs for error messages
* Ensure parameters are within provider limits
* Join the [Hartsy Discord Community](https://discord.gg/nWfCupjhbm) for support

## Changelog
------------

* Version 0.1: Initial release with OpenAI and Anthropic support
* Version 0.2: Added Black Forest Labs integration
* Version 0.3: Added Ideogram support and parameter optimization

## License
----------

This extension is licensed under the [MIT License](https://opensource.org/licenses/MIT).

## Contributing
--------------

Contributions welcome! Please submit Pull Requests or open Issues on GitHub.

## Acknowledgments
-----------------

* [mcmonkey](https://github.com/mcmonkey4eva) for creating SwarmUI
* The API providers for their services and documentation
* The Hartsy development team
* [Hartsy AI](https://hartsy.ai) community for testing and feedback