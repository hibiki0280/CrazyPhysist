# this file can run by a command like python3 render_model.py -t cartpole -D 2018-11-18-18-34-41
import argparse
import sys
import numpy as np

from tensorforce.agents import PPOAgent
from tensorforce.execution import Runner
import gym

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('-t', '--agent-type', help="What kind of agent you wanna render. e.g. bipedal, cartpole")
    parser.add_argument('-D', '--dir', help="Where you wanna render a image or a video from. e.g. 2018-11-18-17-30-30")
    args = parser.parse_args()

    # restore the model in dir
    if args.agent_type == "cartpole":
        from tensorforce.contrib.openai_gym import OpenAIGym
        environment = OpenAIGym('CartPole-v0', visualize=False)
        network_spec = [
            # dict(type='embedding', indices=100, size=32),
            # dict(type'flatten'),
            dict(type='dense', size=32),
            dict(type='dense', size=32)
        ]
        agent = PPOAgent(
            states=environment.states,
            actions=environment.actions,
            network=network_spec,
            # Agent
            states_preprocessing=None,
            actions_exploration=None,
            reward_preprocessing=None,
            # MemoryModel
            update_mode=dict(
                unit='episodes',
                # 10 episodes per update
                batch_size=10,
                # Every 10 episodes
                frequency=10
            ),
            memory=dict(
                type='latest',
                include_next_states=False,
                capacity=5000
            ),
            # DistributionModel
            distributions=None,
            entropy_regularization=0.01,
            # PGModel
            baseline_mode='states',
            baseline=dict(
                type='mlp',
                sizes=[32, 32]
            ),
            baseline_optimizer=dict(
                type='multi_step',
                optimizer=dict(
                    type='adam',
                    learning_rate=1e-3
                ),
                num_steps=5
            ),
            gae_lambda=0.97,
            # PGLRModel
            likelihood_ratio_clipping=0.2,
            # PPOAgent
            step_optimizer=dict(
                type='adam',
                learning_rate=1e-3
            ),
            subsampling_fraction=0.2,
            optimization_steps=25,
            execution=dict(
                type='single',
                session_config=None,
                distributed_spec=None
            )
        )
        agent.restore_model(directory=args.dir)
        env = gym.make("CartPole-v0")
        s = env.reset()
        done = False
        while not done:
            env.render()
            action = agent.act(s)
            s, r, done, _ = env.step(action)
        env.close()
    elif args.agent_type == "bipedal":
        from tensorforce.contrib.unity import UnityEnv
        pass

if __name__ == '__main__':
    main()
