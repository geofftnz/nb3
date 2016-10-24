﻿//|vert
#version 410
precision highp float;
layout (location = 0) in vec3 vertex;
layout (location = 0) out vec2 texcoord;

void main() {

	gl_Position = vec4(vertex.xy,0.0,1.0);
	texcoord = vertex.xy * 0.5 + 0.5;
}



//|frag
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;

void main(void)
{
	//texcoord.y *= texcoord.y;
	vec2 t = texcoord.yx;
	//t.x = t.x * t.x;// + t.x*0.5;
	t.x = pow(t.x,sqrt(2.0));
	float s = texture2D(spectrumTex,t).r;

	//s = s * 128.0 * sqrt(t.x);
	s = 40.0*log(1.0+s);

	vec4 col = vec4(0.0,0.0,0.0,1.0);
	col.b = max(0.0,min(sqrt(s) * 2.0,1.0) - max(0.0,s-0.5));
	col.r = min(max(0.0,s - 0.25) * 4.0,1.0);
	col.g = min(max(0.0,s - 0.8) * 5.0,1.0);

	out_Colour = col;	
}

