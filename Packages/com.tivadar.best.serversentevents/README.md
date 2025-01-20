# Best Server-Sent Events

Welcome to the Best Server-Sent Events Documentation! Best SSE is a premier Unity networking library crafted for seamless integration of the *Server-Sent Events* technology. 
Perfect for games and applications requiring real-time updates such as multiplayer game scores, in-game live events, and dynamic leaderboards.

!!! Warning "**Dependency Alert**"
    Best Server-Sent Events depends on the **Best HTTP** package! 
    Ensure you've installed and configured this package in your Unity project before diving into Best SSE. 
    Explore more about the [installation of Best HTTP](../HTTP/installation.md).

## Overview
In our rapidly updating digital world, delivering real-time data straight to users is crucial for countless applications. 
From live news updates to real-time scoreboards, *Server-Sent Events* empower these updates with consistency and simplicity. 
Best SSE simplifies integrating this technology into your Unity projects, ensuring dynamic **one-way** data streams from server to client.

## Key Features
- **Supported Unity Versions:** Best SSE is compatible with Unity versions starting from :fontawesome-brands-unity: **2021.1 onwards**.
- **Compatibility with SSE Standards:** Best SSE aligns perfectly with the latest Server-Sent Events specifications, guaranteeing cutting-edge real-time data streaming capabilities.
- **Cross-Platform Excellence:** Best SSE is designed to work seamlessly across a diverse range of Unity platforms, ensuring versatility for all your development needs. Specifically, it supports:
    
    - :fontawesome-solid-desktop: **Desktop:** Windows, Linux, MacOS
    - :fontawesome-solid-mobile:  **Mobile:** iOS, Android
    - :material-microsoft-windows: **Universal Windows Platform (UWP)**
    - :material-web: **Web Browsers:** WebGL
    
    This broad platform support means you can confidently use Best SSE, regardless of your target audience or deployment strategy.

- **Auto-Reconnection:** Best Server-Sent Events handles automatic reconnections, guaranteeing continuous user experiences even amidst unstable network scenarios.
- **Seamless Integration:** With intuitive APIs and extensive documentation, integrating Best SSE into any Unity project is hassle-free.
- **Easy-to-Use API:** Designed with simplicity in mind, Best Server-Sent Events offers an intuitive API, making integration and utilization straightforward for newcomers.
- **Performance Optimized:** Best SSE is designed for top-tier performance, ensuring quick data streams with real-time interactions.
- **Event-Driven Data Streaming:** Harness event-based real-time data updates, making your applications current and dynamic.
- **Secure Layers:** Offering encrypted connections, Best SSE guarantees that your application's data streams remain confidential and shielded.
- **Profiler Integration:** Benefit from the in-depth [Best HTTP profiler](../Shared/profiler/index.md) integration:
    - **Memory Profiler:** Examine the internal memory allocations, refining performance and spotting potential memory issues.
    - **Network Profiler:** Oversee your networking nuances, studying data streams and connection statuses.
- **Debugging and Logging:** Comprehensive logging options enable developers to get insights into the workings of the package and simplify the debugging process.

## Documentation Sections
Begin your exploration with Best SSE:

- [Installation Guide:](installation.md) Initiate with Best SSE by integrating the package and preparing your Unity environment.
- [Upgrade Guide:](upgrade-guide.md) Transitioning from a previous version? Grasp the latest features and make your upgrade process smooth.
- [Getting Started:](getting-started/index.md) Embark on your SSE journey, understanding the basics, and customizing Best SSE to fit your application's demands.

Whether you're a seasoned developer or just getting started with Unity, this documentation will guide you through the process of leveraging Best SSE's capabilities to create efficient, feature-rich applications.

Immerse yourself and elevate your Unity projects with unparalleled real-time data streams, all thanks to Best SSE!

## Installation Guide

!!! Warning "Dependency Alert"
    Before installing Best SSE, ensure you have the [Best HTTP package](../HTTP/index.md) installed and set up in your Unity project. 
    If you haven't done so yet, refer to the [Best HTTP Installation Guide](../HTTP/installation.md).

Getting started with Best Server-Sent Events requires a prior installation of the Best HTTP package. Once Best HTTP is set up, integrating Best SSE into your Unity project is a breeze.

### Installing from the Unity Asset Store using the Package Manager Window

1. **Purchase:** If you haven't previously purchased the package, proceed to do so. Once purchased, Unity will recognize your purchase, and you can install the package directly from within the Unity Editor. If you already own the package, you can skip these steps.
    1. **Visit the Unity Asset Store:** Navigate to the [Unity Asset Store](https://assetstore.unity.com/publishers/4137?aid=1101lfX8E) using your web browser.
    2. **Search for Best SSE:** Locate and choose the official Best SSE package.
    3. **Buy Best SSE:** By clicking on the `Buy Now` button go though the purchase process.
2. **Open Unity & Access the Package Manager:** Start Unity and select your project. Head to [Window > Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).
3. **Select 'My Assets':** In the Package Manager, switch to the [My Assets](https://docs.unity3d.com/Manual/upm-ui-import.html) tab to view all accessible assets.
4. **Find Best SSE and Download:** Scroll to find "Best Server-Sent Events". Click to view its details. If it isn't downloaded, you'll notice a Download button. Click and wait. After downloading, this button will change to Import.
5. **Import the Package:** Once downloaded, click the Import button. Unity will display all Best SSE' assets. Ensure all are selected and click Import.
6. **Confirmation:** After the import, Best SSE will integrate into your project, signaling a successful installation.

### Installing from a .unitypackage file

If you have a .unitypackage file for Best SSE, follow these steps:

1. **Download the .unitypackage:** Make sure the `Best Best Server-Sent Events.unitypackage` file is saved on your device. 
2. **Import into Unity:** Open Unity and your project. Go to Assets > Import Package > Custom Package.
3. **Locate and Select the .unitypackage:** Find where you saved the `Best Best Server-Sent Events.unitypackage` file, select it, and click Open.
4. **Review and Import:** Unity will show a list of all the package's assets. Ensure all assets are selected and click Import.
5. **Confirmation:** Post import, you'll see all the Best SSE assets in your project's Asset folder, indicating a successful setup.

!!! Note
    Best SSE also supports other installation techniques as documented in Unity's manual for packages. For more advanced installation methods, please see the Unity Manual on [Sharing Packages](https://docs.unity3d.com/Manual/cus-share.html).

### Assembly Definitions and Runtime References
For developers familiar with Unity's development patterns, it's essential to understand how Best SSE incorporates Unity's systems:

- **Assembly Definition Files:** Best SSE incorporates [Unity's Assembly Definition files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html). It aids in organizing and managing the codebase efficiently.
- **Auto-Referencing of Runtime DLLs:** The runtime DLLs produced by Best SSE are [Auto Referenced](https://docs.unity3d.com/Manual/class-AssemblyDefinitionImporter.html), allowing Unity to automatically recognize and utilize them without manual intervention.
- **Manual Package Referencing:** Should you need to reference Best SSE manually in your project (for advanced setups or specific use cases), you can do so. Simply [reference the package](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#reference-another-assembly) by searching for `com.Tivadar.Best.WebSockets`.

Congratulations! You've successfully integrated Best SSE into your Unity project. Begin your WebSocket adventure with the [Getting Started guide](getting-started/index.md).

For any issues or additional assistance, please consult the [Community and Support page](../Shared/support.md).