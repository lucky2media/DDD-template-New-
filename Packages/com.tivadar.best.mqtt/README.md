# Best MQTT

Best MQTT is a premier Unity networking library, purpose-built for the seamless integration of the [MQTT protocol](https://mqtt.org/). 
Ideal for dynamic real-time experiences like IoT device communication, messaging systems, and live data streams.

## Overview
In today's interconnected world, real-time data exchange is integral to numerous applications, from IoT device communication to instant messaging. 

**MQTT**, or Message Queuing Telemetry Transport, is a highly efficient publish/subscribe messaging transport protocol. 
It's lightweight, making it perfect for communication where bandwidth is a concern.
Unlike the traditional request-response model, MQTT operates on a publisher-subscriber model, ensuring real-time data delivery with minimal overhead.

Best MQTT makes the task of integrating this powerful protocol into your Unity projects straightforward. 
It's optimized for strong bi-directional communication, ensuring your users benefit from a responsive and live data exchange experience.

## Key Features
- **Supported Unity Versions:** Compatible with Unity versions starting from :fontawesome-brands-unity: **2021.1 onwards**.
- **Compatibility with MQTT:** Aligns with the latest versions of MQTT, placing cutting-edge messaging capabilities in your hands.
- **Cross-Platform Mastery:** Operates smoothly across a spectrum of Unity-supported platforms, making it versatile for a wide range of projects. Specifically, it supports:
    - :fontawesome-solid-desktop: **Desktop:** Windows, Linux, MacOS
    - :fontawesome-solid-mobile:  **Mobile:** iOS, Android
    - :material-microsoft-windows: **Universal Windows Platform (UWP)**
    - :material-web: **Web Browsers:** WebGL

    With such broad platform coverage, this library emerges as a top choice for various platform needs.

- **Seamless Integration:** With intuitive APIs and comprehensive documentation, integrating into any Unity project becomes straightforward.
- **Performance Optimized:** Engineered for top-tier performance, ensuring swift data transfers ideal for real-time scenarios.
- **Lightweight and Efficient:** Minimized overheads make it perfect for scenarios with constrained bandwidth.
- **Bi-directional Communications:** Engage in two-way data exchanges effortlessly, enhancing application responsiveness.
- **Reliable Message Delivery:** Ensures messages reach their intended recipients due to its robust delivery mechanisms.
- **Support for Unreliable Networks:** Even in environments with fluctuating connectivity, the library remains steadfast.
- **Topic Subscriptions:** Manage your MQTT subscriptions smoothly, defining channels suited to your application needs.
- **Event-Driven Communication:** Capitalize on event-based communication paradigms to keep your applications engaging and current.
- **Secure Communications:** With support for encrypted connections, your data transmissions are safeguarded.
- **Profiler Integration:** Leverage the comprehensive [Best HTTP profiler](../Shared/profiler/index.md) integration:
    - **Memory Profiler:** Assess memory usage patterns, boost performance, and pinpoint potential memory bottlenecks.
    - **Network Profiler:** Monitor network behavior, analyzing data transfers, connection health, and more.
- **Effective Data Models:** With support for both JSON and binary data, the library offers flexibility in data interactions.
- **Debugging and Logging:** Robust logging tools assist developers in understanding the library's workings and facilitate debugging.

## Installation Guide

!!! Warning "Dependency Alert"
    Before installing Best MQTT, ensure you have the [Best HTTP package](../HTTP/index.md) and the [Best WebSockets package](../WebSockets/index.md) installed and set up in your Unity project. 
    If you haven't done so yet, refer to the [Best HTTP Installation Guide](../HTTP/installation.md) and the [Best WebSockets Installation Guide](../WebSockets/installation.md).

Getting started with Best MQTT demands a prior setup of both the Best HTTP and Best WebSockets packages. 
After ensuring these are properly integrated, you can then effortlessly add Best MQTT to your Unity projects.

### Installing from the Unity Asset Store using the Package Manager Window

1. **Purchase:** If you haven't previously purchased the package, proceed to do so. 
    Once purchased, Unity will recognize your purchase, and you can install the package directly from within the Unity Editor. If you already own the package, you can skip these steps.
    1. **Visit the Unity Asset Store:** Navigate to the [Unity Asset Store](https://assetstore.unity.com/publishers/4137?aid=1101lfX8E) using your web browser.
    2. **Search for Best MQTT:** Locate and choose the official Best MQTT package.
    3. **Buy Best MQTT:** By clicking on the `Buy Now` button go though the purchase process.
2. **Open Unity & Access the Package Manager:** Start Unity and select your project. Head to [Window > Package Manager](https://docs.unity3d.com/Manual/upm-ui.html).
3. **Select 'My Assets':** In the Package Manager, switch to the [My Assets](https://docs.unity3d.com/Manual/upm-ui-import.html) tab to view all accessible assets.
4. **Find Best MQTT and Download:** Scroll to find "Best MQTT". Click to view its details. If it isn't downloaded, you'll notice a Download button. Click and wait. After downloading, this button will change to Import.
5. **Import the Package:** Once downloaded, click the Import button. Unity will display all Best MQTT' assets. Ensure all are selected and click Import.
6. **Confirmation:** After the import, Best MQTT will integrate into your project, signaling a successful installation.

### Installing from a .unitypackage file

If you have a .unitypackage file for Best MQTT, follow these steps:

1. **Download the .unitypackage:** Make sure the Best MQTT.unitypackage file is saved on your device. 
2. **Import into Unity:** Open Unity and your project. Go to Assets > Import Package > Custom Package.
3. **Locate and Select the .unitypackage:** Find where you saved the Best MQTT.unitypackage file, select it, and click Open.
4. **Review and Import:** Unity will show a list of all the package's assets. Ensure all assets are selected and click Import.
5. **Confirmation:** Post import, you'll see all the Best MQTT assets in your project's Asset folder, indicating a successful setup.

!!! Note
    Best MQTT also supports other installation techniques as documented in Unity's manual for packages. 
    For more advanced installation methods, please see the Unity Manual on [Sharing Packages](https://docs.unity3d.com/Manual/cus-share.html).

### Assembly Definitions and Runtime References
For developers familiar with Unity's development patterns, it's essential to understand how Best MQTT incorporates Unity's systems:

- **Assembly Definition Files:** Best MQTT incorporates [Unity's Assembly Definition files](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html). It aids in organizing and managing the codebase efficiently.
- **Auto-Referencing of Runtime DLLs:** The runtime DLLs produced by Best MQTT are [Auto Referenced](https://docs.unity3d.com/Manual/class-AssemblyDefinitionImporter.html), allowing Unity to automatically recognize and utilize them without manual intervention.
- **Manual Package Referencing:** Should you need to reference Best MQTT manually in your project (for advanced setups or specific use cases), you can do so. 
Simply [reference the package](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html#reference-another-assembly) by searching for `com.Tivadar.Best.MQTT`.

Congratulations! You've successfully integrated Best MQTT into your Unity project. Begin your MQTT adventure with the [Getting Started guide](getting-started/index.md).

For any issues or additional assistance, please consult the [Community and Support page](../Shared/support.md).