﻿//|effect
#version 410
precision highp float;
layout (location = 0) in vec2 texcoord;
layout (location = 1) in vec2 pos;
layout (location = 0) out vec4 out_Colour;
uniform sampler2D spectrumTex;
uniform sampler2D spectrum2Tex;
uniform sampler2D audioDataTex;
uniform float currentPosition;
uniform float currentPositionEst;

#include "Common/gamma.glsl";

#define DATARES 256
#define PI 3.1415926535897932384626433832795
#define PIOVER2 1.5707963267948966192313216916398
#define LOGe10 2.30258509299

vec3 log10(vec3 s)
{
	return log(s.rgb) / LOGe10;
}

vec4 todB(vec4 s)
{
	// ignore 4th component (stereo angle)
	s.rgb = 20.0*log10(s.rgb);
	s.rgb = max(vec3(0.0),vec3(1.0) + (s.rgb / vec3(100.0)));
	return s;
}
float todB(float s)
{
	s = 20.0*log(s);
	s = max(0.0,1.0 + ((s+20.0) / 200.0));
	return s;
}


vec3 colscale(float s)
{
	vec3 col  = vec3(0.0,0.0,0.0   );

	vec3 col0 = vec3(0.0,0.0,0.05   );
	vec3 col1 = vec3(0.0,0.1,0.9 );
	vec3 col2 = vec3(0.0,0.6,0.6   );
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

float stexel = 1.0/DATARES;
float ttexel = 1.0/1024.0;

float getDataSample(float index, float offset)
{
	vec2 t = vec2(index * stexel, currentPositionEst - offset * ttexel);
	
	float s = texture2D(audioDataTex,t).r;
	return s;
}

vec2 texel = vec2(1.0/1024.0,0.0);

vec4 getSample(sampler2D spectrum, vec2 t)
{
	vec2 raw = texture2D(spectrum,t).rg;

	float sep = asin((raw.g - raw.r) / (raw.g + raw.r)) / PIOVER2;

	vec4 s = vec4(raw.rg,(raw.r+raw.g)*0.5, sep);

	return s;
}

vec4 getOffsetSample(sampler2D spectrum, float freq, float offset)
{
	return getSample(spectrum, vec2(freq, currentPositionEst - offset * ttexel));
}


vec4 scaleSpectrum(vec4 s)
{
	return todB(s);
}

float fscale(float x)
{
	return x * 0.1 + 0.9 * x * x;
}



void main(void)
{
	vec3 col = vec3(0.0); //vec3(texcoord.xy,0.2);
	//vec3 col = vec3(texcoord.xy,0.2);


	float time_since_pulse = 1.0 - getDataSample(6.0,0.0);

	col += vec3(0.0,0.0,1.0) * exp(-time_since_pulse * 4.0);
	col += vec3(0.0,1.0,0.0) * exp(-time_since_pulse * 6.0);
	col += vec3(1.0,0.0,0.0) * exp(-time_since_pulse * 8.0);
	

	/*
	float r = sqrt(dot(pos,pos));
	float a = (atan(pos.y, pos.x) / PI) * 0.5 + 0.5;

	vec3 bgcol = colscale(4.0*scaleSpectrum(getSample(spectrum2Tex,vec2(a,currentPositionEst - (r) * 0.2 ))).b);

	float time_since_pulse = 1.0 - getDataSample(0.0,0.0);

	float pulse = (1.0 / exp(time_since_pulse * 8.0)) * 0.2;
	float pulse2 = 1.0 / exp(time_since_pulse * 2.0);


	col.rgb = mix(bgcol,vec3(2.0,1.2,1.5) * pulse2,1.0-smoothstep(pulse,pulse + 0.02,r));
	*/


	// spectrum background
	float specy = fscale(texcoord.y);

	float ss = 1.0;
	ss *= max(0.0,scaleSpectrum ( getOffsetSample ( spectrumTex, specy, (texcoord.x) * 7.0)).b  - 0.1) * 4.0;
	ss *= max(0.0,scaleSpectrum ( getOffsetSample ( spectrumTex, specy, (texcoord.x) * 19.0)).b  - 0.1) * 4.0;
	ss *= max(0.0,scaleSpectrum ( getOffsetSample ( spectrumTex, specy, (1.0-texcoord.x) * 71.0)).b  - 0.1) * 4.0;
	col += vec3(0.0,0.1,0.4) * ss * 0.02;

	ss = 1.0;
	specy = fscale(1.0-texcoord.y);
	ss *= max(0.0,scaleSpectrum ( getOffsetSample ( spectrumTex, specy, (texcoord.x) * 7.0)).b  - 0.1) * 4.0;
	ss *= max(0.0,scaleSpectrum ( getOffsetSample ( spectrumTex, specy, (texcoord.x) * 19.0)).b  - 0.1) * 4.0;
	ss *= max(0.0,scaleSpectrum ( getOffsetSample ( spectrumTex, specy, (1.0-texcoord.x) * 71.0)).b  - 0.1) * 4.0;

	col += vec3(0.0,0.1,0.4) * ss * 0.02;

	//col += vec3(0.0,0.1,0.4) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (1.0-texcoord.x) * 200.0)).b  - 0.1);
	//col += vec3(0.0,0.1,0.4) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (texcoord.x) * 50.0)).b  - 0.1);
	//col += vec3(0.0,0.1,0.4) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (texcoord.x) * 25.0)).b  - 0.1);
	//col += vec3(0.0,0.0,0.3) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex,specy, (texcoord.x) * 50.0)).b  - 0.2);
	//col += vec3(0.0,0.1,0.4) * smoothstep(0.05,0.4,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (texcoord.x) * 20.0)).b);

	//specy = fscale(1.0-texcoord.y);
	//col += vec3(0.0,0.1,0.4) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (1.0-texcoord.x) * 200.0)).b  - 0.1);
	//col += vec3(0.0,0.1,0.4) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (texcoord.x) * 50.0)).b  - 0.1);
	//col += vec3(0.0,0.1,0.4) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (texcoord.x) * 25.0)).b  - 0.1);
	//col += vec3(0.0,0.0,0.25) * max(0.0,scaleSpectrum ( getOffsetSample ( spectrum2Tex,specy, (texcoord.x) * 50.0)).b  - 0.2);
	//col += vec3(0.0,0.1,0.4) * smoothstep(0.05,0.4,scaleSpectrum ( getOffsetSample ( spectrum2Tex, specy, (texcoord.x) * 20.0)).b);



	for (float x = 0.0; x <= 1.01; x+= 0.05)
	//for (float x = texcoord.x - 0.2; x <= texcoord.x + 0.2; x+= 0.1)
	{
		if (x < texcoord.x - 0.1 || x > texcoord.x + 0.1)
			continue;

		// draw a circle (x,y,r)
		vec3 circle = vec3(x,0.1,0.02);
		vec3 ccol = vec3(0.0,0.1,1.0);

		float time = (1.0-x) * 100.0;

		float freq = getDataSample(0.0,time);
		circle.y = 0.1 + 0.8 * sqrt(freq);
		circle.z = 0.0001 + 0.3 * max(0.0,getDataSample(3.0,time)-0.3) ;
		ccol = colscale(3.0 * max(0.0,getDataSample(3.0,time) - 0.2));

		// get distance from centre
		float d = length(texcoord.xy-circle.xy) - circle.z;
	
		float cinside = smoothstep(-circle.z,0.0,d);		
		col += ccol * (1.0-smoothstep(0.0,0.01,d)) * cinside * cinside;
	}
	
	// gamma
	col.rgb = l2g(col.rgb);

	out_Colour = vec4(col,1.0);	
}

