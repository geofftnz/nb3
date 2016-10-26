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
uniform float currentPosition;

float getSample(vec2 t)
{
	float s = texture2D(spectrumTex,t).r;

	//s = 40.0*log(s+1.0);

	//s = 4.0 * sqrt(s);

	s = 20.0*log(s);
	s = max(0.0,1.0 + ((s+20.0) / 200.0));

	//s = 5.0 * pow(s,1.0/sqrt(2.0));

	return s;
}

vec2 texel = vec2(1.0/1024.0,0.0);

float fscale(float x)
{
	return x * 0.1 + 0.9 * x * x;
}

vec3 g2l(vec3 c)
{
	return pow(c,vec3(2.0));
}
vec3 l2g(vec3 c)
{
	return pow(c,vec3(1.0/2.0));
}

vec3 colscale(float s)
{
	vec3 col  = vec3(0.0,0.0,0.0   );

	vec3 col0 = vec3(0.0,0.0,0.05   );
	vec3 col1 = vec3(0.0,0.1,0.9 );
	vec3 col2 = vec3(0.0,0.8,0.6   );
	vec3 col3 = vec3(0.9,0.9,0.0  );
	vec3 col4 = vec3(0.9,0.0,0.0  );
	vec3 col5 = vec3(1.0,1.0,1.0);
	
	col = mix(col0,col1,clamp(s*2.5,0.0,1.0));
	col = mix(col,col2,clamp((s-0.3)*2.0,0.0,1.0));
	col = mix(col,col3,clamp((s-0.5)*4.0,0.0,1.0));
	col = mix(col,col4,clamp((s-0.6)*5.0,0.0,1.0));
	col = mix(col,col5,clamp((s-0.8)*6.0,0.0,1.0));

	return col;
}

vec4 renderSpectrum(vec2 t)
{
	t.x = fscale(t.x);

	float s = getSample(t);

	vec3 col = colscale(s);

	float a = 1.0 - smoothstep(abs(currentPosition-t.y),0.0,0.5/1024.0);
	col += vec3(0.8) * a;

	return vec4(col,1.0);
}

vec4 renderGraph(vec2 t)
{
	vec3 col = vec3(0.0,0.0,0.0);
	float ty = fscale(t.y);

	for (float i = 0.0;i<1.0;i+=0.1)
	{
		float s = getSample(vec2(ty,currentPosition - texel.x * i * 4.0));

		float a = abs(s-(t.x+i*0.1));
        float w = 0.05 - step(0.01,i) * 0.03;

        a = 1.0 - smoothstep(0.0,w,a);
		//a = 1.0 - step(w,a);
		col += colscale(s) * a * 0.7;// * (1.0 - smoothstep(0.03,0.9,i) * 0.9);
	}

	return vec4(col,1.0);
}


void main(void)
{
	//texcoord.y *= texcoord.y;
	vec2 t = texcoord.yx;

	vec4 col;

	if (t.x>0.2)
	{
		vec2 t2 = vec2((t.x-0.2)/0.8,t.y);
		col = renderSpectrum(t2);
	}
	else
	{
		col = renderGraph(vec2(t.x*5.0,t.y));
	}

	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = col;	
}

