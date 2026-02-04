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
7. [Architecture](#architecture)
8. [API](#api)
9. [Troubleshooting](#troubleshooting)
10. [Changelog](#changelog)
11. [License](#license)
12. [Contributing](#contributing)
13. [Acknowledgments](#acknowledgments)

## Introduction
---------------

The APIBackends Extension for SwarmUI enables integration with multiple commercial image generation APIs. It registers API-hosted models into SwarmUI's normal model list so they can be selected and used from the **Generate** tab like local models.

* Usage Example:
- Generate images using DALL-E 3
- Access Black Forest Labs' Flux models
- Generate with Ideogram's AI
- Use OpenAI GPT Image models
- Use Grok, Google (Imagen / Gemini), and Fal.ai models
- Seamlessly switch between different API providers

> [!WARNING]
> API usage incurs costs from the respective providers. Make sure you understand the pricing before use.
> Always keep your API keys secure and never share them.

## Features
------------

* Support for multiple API providers:
  - OpenAI (DALL-E 2, DALL-E 3, GPT Image models)
  - Ideogram
  - Black Forest Labs (FLUX)
  - Grok
  - Google (Imagen / Gemini)
  - Fal.ai
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

1. Enable the **3rd Party Paid API Backends** backend in SwarmUI
2. In that backend's settings, enable one or more provider checkboxes (OpenAI / Ideogram / BFL / Grok / Google / Fal)
3. Add the corresponding API key(s) in the **User** tab (API key manager)
4. Go to **Generate** and pick a model under:
   - `API Models/<Provider>/<Model>`
5. Adjust the provider-specific parameters (they appear automatically based on the selected model)
6. Generate as normal

## Configuration
----------------

This extension uses two configuration surfaces:

- **Backend settings** (Server-side)
  - Enable/disable providers via checkboxes
  - Optional: set a custom base URL override
- **User API keys** (Per-user)
  - Each provider has a dedicated API key entry
  - Keys are stored in SwarmUI user data (never hardcode keys in code)

### Providers

Each API provider requires an API key:

* OpenAI:
  - API Key
  - Model selection (DALL-E 2, DALL-E 3, GPT Image)
  - Quality and style parameters

* Black Forest Labs:
  - API Key
  - Model selection
  - Custom endpoint (optional)

* Ideogram:
  - API Key
  - Style preferences
  - Resolution settings

* Grok:
  - API Key
  - Model selection

* Google:
  - API Key
  - Model selection (Imagen / Gemini)

* Fal.ai:
  - API Key
  - Model selection

## Architecture
---------------

This extension uses a data-driven factory pattern to keep providers modular and scalable (including providers with hundreds of models).

- **Provider definitions**
  - `Providers/ProviderDefinitions.cs` defines providers and their model catalogs via `ProviderDefinition` + `ModelDefinition`.
- **Model factory**
  - `Models/ModelFactory.cs` converts `ModelDefinition` to SwarmUI `T2IModel` instances.
- **Request builders**
  - `Providers/RequestBuilders.cs` contains provider-specific request/response implementations.
- **Provider initialization/registry**
  - `Backends/APIProviderInit.cs` initializes provider metadata and models.
  - `APIProviderRegistry.Instance` exposes the initialized providers.
- **Runtime backend**
  - `Backends/DynamicAPIBackend.cs` is the Swarm backend that executes requests against the active provider.

### Model naming / UI grouping

API models are registered with names like:

- `API Models/<Provider>/<ModelId>`

This groups all API-backed models under a single top-level folder in the model selector.

## API
------

This extension does **not** add brand-new HTTP routes. Instead, it integrates into SwarmUI's existing WebAPI in two places:

### 1) Model listing and metadata (ModelsAPI)

The backend registers an extra model provider via `ModelsAPI.ExtraModelProviders["dynamic_api_backends"]`. This makes API models appear as **remote models** in the normal model browser.

Relevant Swarm API calls (names as registered by SwarmUI; typically available at `/API/<CallName>`):

- **`ListModels`**
  - Purpose: list models in folders (includes API models when `allowRemote=true`)
  - Key inputs:
    - `path` (folder)
    - `depth`
    - `subtype` (usually `Stable-Diffusion`)
    - `allowRemote` (must be `true` to include API models)
- **`DescribeModel`**
  - Purpose: get metadata for a single model (works for API models too)
  - Key inputs:
    - `modelName`
    - `subtype`

### 2) Generate tab params + generation (T2IAPI)

Provider-specific parameters are registered into SwarmUI's parameter system and are returned through:

- **`ListT2IParams`**
  - Purpose: returns all T2I parameters, param groups, and model lists used by the Generate tab.

Actual generation uses SwarmUI's standard generation endpoints. This extension participates by providing a backend that can service requests for models under `API Models/...`:

- **`GenerateText2Image`**
- **`GenerateText2ImageWS`** (WebSocket live updates)

In requests, set the `model` parameter to an API model name, for example:

- `model: "API Models/Ideogram/V_3"`
- `model: "API Models/BFL/flux-2-max"`

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

* Version 1.1: Modular provider/model factory architecture, unified `API Models/<Provider>/...` model naming

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