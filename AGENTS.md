# Agent Loop

O projeto possui dois agentes locais e determinísticos. Eles não fazem chamadas de rede.

## CriticAgent

Avalia o capítulo atual em cinco dimensões:
1. Hook inicial.
2. Ritmo de parágrafos.
3. Presença de diálogo/voz.
4. Repetição lexical aproximada.
5. Força do encerramento/cliffhanger.

Retorna no máximo três observações, priorizadas por impacto.

## BuilderAgent

Recebe a crítica e converte a observação principal em uma micro-missão concreta, por exemplo:
- “Escreva 120 palavras que deixem explícito o objetivo imediato.”
- “Adicione uma fala que revele conflito sem explicar o backstory.”
- “Reescreva as duas últimas frases para abrir uma pergunta.”

## Protocolo de melhoria do repositório

Para futuras iterações humanas ou com agente de código:
1. Builder implementa uma mudança pequena e testável.
2. Critic revisa UX de escrita, acoplamento, performance e diversão.
3. Builder corrige apenas os problemas de maior impacto.
4. Registrar a mudança em commit separado.
5. Não aumentar complexidade se não melhorar o fluxo do escritor.
