# DADIU 2024 - Graduation Game

### Git structure

**Main** --- main branch with last functional release (we will merge develop into master for every iteration).   
**Develop** --- working branch, most commits will come from feature branches but occasionaly there will be fix or update commits   
**Feature Branches** --- every time you are in charge of a feature you create your own feature branch following by calling 'feature/$ticket-number-$brief_description' (please use kebab-case for the branch name), where ticket-number and brief description comes from whatever Agile tool we will use. When you are done with the feature you have to rebase onto develop and MUST create a pull request for merging your code, by including at least one programmer as a reviewer (generally invlude @msel)  

### What's the folder structure?

Please follow this guideline https://github.com/justinwasilenko/Unity-Style-Guide

### What's the coding style?

The coding style used in the project originates from the standard Unity coding style, more information can be found
at https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity or https://unity.com/how-to/formatting-best-practices-c-scripting-unity.

Examples can be seen at this git repo: https://github.com/thomasjacobsen-unity/Unity-Code-Style-Guide/tree/master

### How should I write a commit?

In order to keep a consistent structure please use this guideline for commits https://gist.github.com/robertpainsi/b632364184e70900af4ab688decf6f53.

### Which Unity version should I use?

The project was created with Unity LTS 2022.3.43f1 so you MUST use that version for consistency, if there is any kind of problem or special request th team will discuss if it's worth upgrading to a newer version.

### Which IDE should I use?

You are free to choose whatever IDE you prefer :) but Visual Studio/Rider are recommended. 
