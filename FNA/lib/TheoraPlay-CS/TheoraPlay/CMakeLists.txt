# TheoraPlay CMake File
# Written by Ethan "flibitijibibo" Lee
CMAKE_MINIMUM_REQUIRED(VERSION 2.8)
PROJECT(TheoraPlay)

IF (APPLE)
	SET(CMAKE_OSX_ARCHITECTURES i386 x86_64)
ENDIF()

# Compiler flags
SET(CMAKE_C_FLAGS "-O2 -std=c99 -Wall -Wno-implicit-function-declaration")

# Linker flags
IF(${CMAKE_SYSTEM_NAME} MATCHES "Linux")
	SET(CMAKE_SHARED_LINKER_FLAGS "-Wl,--no-undefined")
ENDIF(${CMAKE_SYSTEM_NAME} MATCHES "Linux")

# Include our own API...
INCLUDE_DIRECTORIES(include)

# Target
ADD_LIBRARY(theoraplay SHARED src/theoraplay.c)
TARGET_LINK_LIBRARIES(theoraplay ogg)
TARGET_LINK_LIBRARIES(theoraplay vorbis)
TARGET_LINK_LIBRARIES(theoraplay theora)
TARGET_LINK_LIBRARIES(theoraplay theoradec)
TARGET_LINK_LIBRARIES(theoraplay pthread)
