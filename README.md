# Electric Field Visualizer

This project is a 2D electric field simulator implemented in **C#** using **Windows Forms**. It visually demonstrates how electric fields are formed by point charges, allowing users to interactively place charges, simulate time-varying effects, and observe field behaviors in various visual forms.

## Project Background
Developed as a semester project for the course **UPG** at the University of West Bohemia, this application aims to provide an interactive and educational tool for understanding electrostatic principles. The implementation includes several advanced features such as splines, dynamic charges, customizable scenarios, and graph-based analysis.

## Requirements
- **.NET Framework 4.7.2 or later**
- **Visual Studio 2022 or later**
- **Windows OS** (due to WinForms dependency)

## Installation & Setup
### Clone the Repository
```sh
git clone https://github.com/JakubStrunc/eletric-field-vizulizer.git
cd eletric-field-vizulizer
```

### Build & Run Instructions
Just use the included command scripts:
```sh
Build.cmd     # Compiles the project
Run.cmd       # Launches the application
```
No extra setup is needed—just double-click `Build.cmd`, then `Run.cmd`.

You can run a scenario with an optional grid step using:
```sh
Run <scenarioID> -g<X>x<Y>
```
Where `<scenarioID>` is a number (0–4), and `<X>`, `<Y>` are optional grid resolutions.

#### Alternatively, use Visual Studio:
1. Open the `.sln` file in **Visual Studio**.
2. Ensure target framework is installed.
3. Click **Build** → **Run**.

## App Control
### Use Side Menu (arrow button on the right)
- **Scenarios**:
  - **Scenario 0**: Single charge +1C at (0, 0)
  - **Scenario 1**: Two charges +1C at (-1, 0) and +1C at (1, 0)
  - **Scenario 2**: Two charges -1C at (-1, 0) and +2C at (1, 0)
  - **Scenario 3**: Four charges: +1C at (-1, -1), +2C at (1, -1), -3C at (1,1), and -4C at (-1,1)
  - **Load Scenarios**: Select JSON files defining custom charge configurations. The JSON format supports both constant and time-dependent charges, enabling the simulation of dynamic field changes.
```json
[
  {
    "Q": "sin ( t )",
    "PositionX": -1,
    "PositionY": 0
  },
  {
    "Q": "-1",
    "PositionX": 1,
    "PositionY": 0
  }
]
```
Each charge in the scenario is defined by the following parameters:
- **Q**: The charge magnitude, which can be either a constant or a time-dependent expression. If the charge varies over time, the expression should be formatted correctly with spaces, such as `"1 * cos ( t )"` instead of `"1*cos(t)"`.
- **PositionX**: The X-coordinate of the charge in the two-dimensional simulation space.
- **PositionY**: The Y-coordinate of the charge in the two-dimensional simulation space.


- **Simulation Speed**: Adjust speed settings

- **Modes**:
  - **Color Map**: Enable color-coded visualization of field intensity.
  - **Arrows**: Display field direction using vector arrows.
  - **Grid**: Toggle background grid display.
  - **Field Lines**: Show lines representing field direction.
  - **Isocontours**: Display contour lines representing equal intensity regions.

- **Objects**:
  - adding other objects to 0,0 such as positive charge, negative charge or probe

- **Show Graph**:
  - Enable graphing of electric field intensity over time for multiple probes.
  - To add probe to the graph. Double-click on the probe, and check the `Enable Graph`

- **Path**:
  - Assign a movement trajectory to charges using Bézier splines.
  - Ensure paths are valid (invalid paths appear red).

### Other Simulation Controls
- **Add Probes**: Shift + Click anywhere in the simulation to add a static probe.
- **Move Charges/Probes**: Click and drag charges or probes to reposition them.
- **Edit Objects**: Double-click on a charge or probe to open the modification window.
- **Follow Path**: After assigning a path, double-click a charge and check 'Follow Path' to enable movement.
  
## License
This project is licensed under the MIT License.
