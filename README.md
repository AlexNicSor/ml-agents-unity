**# Project Plan: Ai and Machine Learning with Unity Ml-Agents
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
4. [Project Structure](#project-structure)
5. [Getting Started](#getting-started)
6. [Installation Guide](#installation-guide)
7. [Project Phases](#project-phases)
   - [Phase 2 Tasks](#phase-2-tasks)
   - [Phase 3 Tasks](#phase-3-tasks)
8. [Risk Analysis](#risk-analysis)
9. [Usage Instructions](#usage-instructions)
10. [References and Resources](#references-and-resources)
11. [Project Contributors](#Contributors)

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

# Project Structure
TODO

# Getting Started
### Prerequisites
- **Git**: Version control software used to manage the respository
- **Unity game Engine**: Required for 3D stimulation
- **Python(v3.10.x)**: Used for Ml-Agents
- **Ml-Agents Toolkit**: Used for training and manging agents

# Installation Guide
1. **Clone The Repository**:
```bash
git clone --branch fix-numpy-release-21-branch https://github.com/AlexNicSor/ml-agents-unity.git
cd ml-agents-unity
```
2. **Set Up Virtual Environment**
```bash
python -m venv venv
source venv\\Scripts\\activate
```
3. **Install Python Dependencies**:
```bash
pip install -r requirements.txt
```
4. **Install Unity**: Download and install [Unity](unity.com)
# Project Phases

# Risk Analysis
1. **Lack of Experience**: Some team members are new to Unity and Ml-Agents, this will pose a challenge
2. **Computational Constraints**: Training deep RLM can be resource-intensive and may require significant computational power
3. **Time Management**:Balancing the project tasks effectively will be important, since we are occupied with different courses 
4. **Debugging Issues**:Debugging issues with complex agents, environments and Unity might be challenging and time-consuming

# Usage Instruction

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
