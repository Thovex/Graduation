#pragma once

#define for3(XSize, YSize, ZSize, Func) for ( int32 X = 0; X < XSize; X++ ) {for ( int32 Y = 0; Y < YSize; Y++ ) { for ( int32 Z = 0; Z < ZSize; Z++ )  { Func } } }