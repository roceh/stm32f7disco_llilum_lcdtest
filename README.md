<b>Simple demo of using llilum to draw some rectangles in c# onto the STM32F7 discovery boards LCD.</b>

You will need to build the SDKDrop in https://github.com/roceh/llilum and add in the vsstudio plugin that generates (see microsofts main llilum wikis for how to).

You will have to manually deploy the bin file (drag/drop in drive it mounts), as unforunately ST don't use CMSIS DAP :(, you can debug kinda of with openocd (i found this one works http://gnutoolchains.com/arm-eabi/openocd/).

