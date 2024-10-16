**Note:** This repository is based on [a ml-agents branch](https://github.com/DennisSoemers/ml-agents/tree/fix-numpy-release-21-branch).

## Overview

This is an **AI and Machine Learning with Unity ML-Agents** project!
This project aims to explore, test, optimize Deep Reinforcement 
Learning (DRL) algorithms for controlling agents in Unity game engine.
We will develop custom sensors, experiment with environments and 
document our results.

## Table of Contents
1. [Introduction](#introduction)
2. [Objectives](#objectives)
3. [Project Description](#project-description)
4. [Getting Started](#getting-started)
5. [Installation Guide](#installation-guide)
6. [Project Phases](#project-phases)
   - [Phase 2 Tasks](#phase-2-tasks)
   - [Phase 3 Tasks](#phase-3-tasks)
7. [Risk Analysis](#risk-analysis)
8. [Usage Instructions](#usage-instructions)
9. [References and Resources](#references-and-resources)
10. [Project Contributors](#Contributors)

# Introduction
This project is about putting **Deep Reinforcement Learning (DRL)** to work in Unity.
We will be using Unity's Ml-Agents (Unity-Technologies, n.d.) to build agents 
that can fulfill different tasks in 3D game. 

# Objectives
The main objectives of this project are as follows:

1. Implement Deep Reinforcement Learning (DRL) using the ML-Agents Toolkit, train
agents in pre-made 3D games and fine-tune the models to achieve better performance 
2. Design and implement new sensors inputs for agents in Unity to enhance their 
ability to interact and perceive the environment and make the pre-made models mimic
the real world.
3. Evaluate the performance of the created algorithm using metrics, training time, 
resource usage, the framerate of the stimulation and optimize agents behaviour by 
exploring different environment complexity, sensor configurations and hyperparameters

# Project Description
For this project, we will be using **Unity Game Engine** with the **Ml-Agents** toolkit 
to create a 3D stimulation environment. Unity will handle the visual aspects
and the in-game logic. **Ml-Agents**, written in Python, will manage the 
decision-making process. By combining those two we will explore and analyse the DRL techniques
in real-time simulations and hypertune the agents through thorough testing

### Key Software Components 
- **Unity**: Used for creating, stimulating and publishing 3D (and 2D) games
- **Ml-Agents Toolkit**: Used to integrate Unity and Machine Learning Agent behaviours
- **C#**: handles the game mechanics using Unity scripts
- **Python**: Runs the machine learning algorithms

# Getting Started
### Prerequisites
- **Git**: Version control software used to manage the respository
- **Unity game Engine**: Required for 3D stimulation
- **Python(v3.10.x)**: Used for Ml-Agents
- **Ml-Agents Toolkit**: Used for training and manging agents

# Installation Guide
1. **Clone The Repository**:
```bash
git clone https://github.com/AlexNicSor/ml-agents-unity.git
cd ml-agents-unity
```
2. **Set Up Virtual Environment**
```bash
python -m venv venv
MAC/Linux: source venv/bin/activate
Windows: venv\Scripts\activate
```
3. **Install ML-Agents**:
```bash
pip install --upgrade pip
pip install -e ./ml-angents-envs
pip install -e ./ml-angents
```
3. **Test instalation**:
```bash
ml-agents-learn --help
```
3. **Additional dependencies**:
```bash
pip install torch torchvision torchaudio
```
4. **Install Unity**: Download and install [Unity](unity.com)

# Project Phases
Our future plans for this project are split into two phases and include the following tasks:

- # Phase 2 Tasks
* Implement Baseline Algorithm for Scenario Adaptation.
* Parameter Adjustment for Algorithm Optimization.
* Code Addition for New Input Type in Soccer Twos
* Peripheral Vision and Advanced Input Simulation
- # Phase 3 Tasks
* Establish Baseline Performance of Algorithm in a selected envi-
ronment(Chosen Game)
* Experiment with Parameter Tuning
* Experiment with different sensor/input types

# Risk Analysis
1. **Lack of Experience**: Some team members are new to Unity and Ml-Agents, this will pose a challenge
2. **Computational Constraints**: Training deep RLM can be resource-intensive and may require significant computational power
3. **Time Management**:Balancing the project tasks effectively will be important, since we are occupied with different courses 
4. **Debugging Issues**:Debugging issues with complex agents, environments and Unity might be challenging and time-consuming

# Usage Instructions
To use the cloned repository, after all other installation steps are completed: 
1. Open unity hub.
2. Press the add button and select the add from disk option. 
3. Navigate to the location of the cloned repository.
4. Select the folder named Project and add it.
5. Open the project.
6. Select the examples folder located at the bottom left of your screen.
7. Select the example game that you want to run.
8. Select the Scences folder and then the scene you want to run.
9. Press play, located at the top middle part of your screen.

# References and Resources 
1. Unity-Technologies. (n.d.-b). GitHub - Unity-Technologies/ml-agents: The Unity Machine Learning Agents Toolkit (ML-Agents) is an open-source project that enables games and simulations to serve as environments for training intelligent agents using deep reinforcement learning and imitation learning. GitHub. https://github.com/Unity-Technologies/ml-agents
2. Unity documentation. (n.d.). Unity Documentation. https://docs.unity.com/
3. Van Rossum, G., & Drake, F. L. (2009). Python 3 Reference Manual. In CreateSpace eBooks. https://dl.acm.org/citation.cfm?id=1593511

# Contributors
This project was developed by the following group of Maastricht University computer
science students:

* Alexandru Lazarina
* Karol Plandowski
* Marios Petrides
* Carl Balagtas
* Marcel Pendyk
* Ethan de Beer
* Hadi Ayoub
