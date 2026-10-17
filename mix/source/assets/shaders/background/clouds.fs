#version 330 core

/*
  Copyright 2026 stopmind

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

uniform float u_time;
uniform sampler2D texture0;

const float GRID_SIZE = 40.0;
const float FRAG_SIZE = GRID_SIZE / 2.0;

ivec2 get_tile(vec2 pos) {
  return ivec2(pos / GRID_SIZE);
}

vec2 get_tile_center(ivec2 tile) {
  return vec2(tile) * GRID_SIZE + vec2(GRID_SIZE / 2.);
}

float tile_effect(vec2 pos, ivec2 tile) {
  float radius = texelFetch(texture0, tile, 0).r * GRID_SIZE * 4.;
  float dist = distance(pos, get_tile_center(tile));
  return max(0.f, 1.f - dist/radius);
}

float clouds(vec2 pos) {
  ivec2 tile = get_tile(pos);
  pos += vec2(GRID_SIZE * 4.0);

  ivec2 size = textureSize(texture0, 0);
  if (size.x <= tile.x || size.y <= tile.y) {
    return 1000.0;
  }

  float value = 0.f;

  for (int x = 0; x <= 8; x++) {
    for (int y = 0; y <= 8; y++) {
      value += tile_effect(pos, tile + ivec2(x, y));
    }
  }

  return value / 5.f;
}

void main(){
  vec2 pos = fragTexCoord;
  pos -= mod(pos, FRAG_SIZE);

  float value = clouds(pos);
  finalColor = vec4(vec3(value - mod(value, 0.2f)) * 0.1f, 1.);
  if (value == 1000.0) {
    finalColor = vec4(1.0, 0.0, 0.0, 1.0);
  }
}